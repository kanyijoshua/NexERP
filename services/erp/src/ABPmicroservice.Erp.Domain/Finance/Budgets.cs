using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>One column of a matrix: a date range, both ends included.</summary>
public sealed record AnalysisPeriod(DateTime Start, DateTime End, string Name)
{
    public bool Contains(DateTime date) => date.Date >= Start && date.Date <= End;
}

/// <summary>Builds consecutive periods of a length (day, week, month, quarter, year) from a date.</summary>
public static class AnalysisPeriods
{
    public static List<AnalysisPeriod> Build(DateTime startDate, AnalysisPeriodType type, int count)
    {
        if (count < 1 || count > ErpDomainConsts.MaxAnalysisPeriods)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.InvalidPeriodCount).WithData("max", ErpDomainConsts.MaxAnalysisPeriods);
        }

        var start = Align(startDate.Date, type);
        var periods = new List<AnalysisPeriod>();
        for (var i = 0; i < count; i++)
        {
            var next = Next(start, type);
            periods.Add(new AnalysisPeriod(start, next.AddDays(-1), Name(start, type)));
            start = next;
        }

        return periods;
    }

    private static DateTime Align(DateTime date, AnalysisPeriodType type) =>
        type switch
        {
            AnalysisPeriodType.Week => date.AddDays(-(((int)date.DayOfWeek + 6) % 7)),
            AnalysisPeriodType.Month => new DateTime(date.Year, date.Month, 1),
            AnalysisPeriodType.Quarter => new DateTime(date.Year, (date.Month - 1) / 3 * 3 + 1, 1),
            AnalysisPeriodType.Year => new DateTime(date.Year, 1, 1),
            _ => date,
        };

    private static DateTime Next(DateTime start, AnalysisPeriodType type) =>
        type switch
        {
            AnalysisPeriodType.Week => start.AddDays(7),
            AnalysisPeriodType.Month => start.AddMonths(1),
            AnalysisPeriodType.Quarter => start.AddMonths(3),
            AnalysisPeriodType.Year => start.AddYears(1),
            _ => start.AddDays(1),
        };

    private static string Name(DateTime start, AnalysisPeriodType type) =>
        type switch
        {
            AnalysisPeriodType.Week => $"W{System.Globalization.ISOWeek.GetWeekOfYear(start):00} {System.Globalization.ISOWeek.GetYear(start)}",
            AnalysisPeriodType.Month => start.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture),
            AnalysisPeriodType.Quarter => $"Q{(start.Month - 1) / 3 + 1} {start.Year}",
            AnalysisPeriodType.Year => start.Year.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => start.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
        };
}

/// <summary>A row of the budget matrix: an account and its budget (and actual) per period.</summary>
public sealed record BudgetMatrixRow(string AccountNo, string AccountName, GLAccountType AccountType, int Indentation, decimal[] Budget, decimal[] Actual);

/// <summary>
/// The G/L budget as a matrix of accounts by periods: reading it, typing one cell (which replaces
/// the budget entries of that account in that period by one), copying a budget, and comparing it
/// with what was actually posted.
/// </summary>
public class BudgetManager : DomainService
{
    private readonly IRepository<GLBudgetName, Guid> _budgetRepository;
    private readonly IRepository<GLBudgetEntry, Guid> _entryRepository;
    private readonly IRepository<GLAccount, Guid> _accountRepository;
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;

    public BudgetManager(
        IRepository<GLBudgetName, Guid> budgetRepository,
        IRepository<GLBudgetEntry, Guid> entryRepository,
        IRepository<GLAccount, Guid> accountRepository,
        IRepository<GLEntry, Guid> glEntryRepository
    )
    {
        _budgetRepository = budgetRepository;
        _entryRepository = entryRepository;
        _accountRepository = accountRepository;
        _glEntryRepository = glEntryRepository;
    }

    public async Task<GLBudgetName> GetBudgetAsync(string budgetName)
    {
        var name = budgetName?.Trim().ToUpperInvariant();
        return await _budgetRepository.FirstOrDefaultAsync(b => b.Name == name)
            ?? throw new BusinessException(ErpErrorCodes.FinanceOperations.BudgetNotFound).WithData("name", name ?? "");
    }

    /// <summary>
    /// The matrix: every posting account (or those whose no. starts with <paramref name="accountFilter"/>)
    /// with its budget per period, and the actual net change when <paramref name="withActual"/>.
    /// </summary>
    public async Task<(List<AnalysisPeriod> Periods, List<BudgetMatrixRow> Rows)> GetMatrixAsync(
        string budgetName,
        DateTime startDate,
        AnalysisPeriodType periodType,
        int periodCount,
        string accountFilter,
        bool withActual,
        bool incomeStatementOnly
    )
    {
        var budget = await GetBudgetAsync(budgetName);
        var periods = AnalysisPeriods.Build(startDate, periodType, periodCount);
        var from = periods[0].Start;
        var to = periods[^1].End;

        var accounts = (await _accountRepository.GetListAsync())
            .Where(a => accountFilter.IsNullOrWhiteSpace() || a.No.StartsWith(accountFilter.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(a => !incomeStatementOnly || a.IncomeBalance == IncomeBalanceType.IncomeStatement)
            .OrderBy(a => a.No)
            .ToList();

        var entries = await _entryRepository.GetListAsync(e => e.BudgetName == budget.Name && e.Date >= from && e.Date <= to);
        var actual = withActual ? await _glEntryRepository.GetListAsync(e => e.PostingDate >= from && e.PostingDate <= to) : [];

        var rows = accounts
            .Select(a => new BudgetMatrixRow(
                a.No,
                a.Name,
                a.AccountType,
                a.Indentation,
                periods.Select(p => entries.Where(e => e.GLAccountNo == a.No && p.Contains(e.Date!.Value)).Sum(e => e.Amount)).ToArray(),
                periods.Select(p => actual.Where(e => e.GLAccountNo == a.No && p.Contains(e.PostingDate)).Sum(e => e.Amount)).ToArray()
            ))
            .ToList();

        return (periods, rows);
    }

    /// <summary>
    /// Sets the budget of one account for one period: the entries already there are replaced by one
    /// entry on the first day of the period. Zero clears the cell.
    /// </summary>
    public async Task SetCellAsync(string budgetName, string accountNo, DateTime periodStart, DateTime periodEnd, decimal amount)
    {
        var budget = await GetBudgetAsync(budgetName);
        if (budget.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.BudgetBlocked).WithData("name", budget.Name);
        }

        var no = accountNo?.Trim();
        var account = await _accountRepository.FirstOrDefaultAsync(a => a.No == no)
            ?? throw new BusinessException(ErpErrorCodes.GLAccounts.GLAccountNotFound).WithData("accountNo", no ?? "");

        var start = periodStart.Date;
        var end = periodEnd.Date;
        await _entryRepository.DeleteAsync(e => e.BudgetName == budget.Name && e.GLAccountNo == account.No && e.Date >= start && e.Date <= end);

        if (amount != 0m)
        {
            await InsertAsync(budget.Name, account.No, start, amount, null);
        }
    }

    /// <summary>
    /// Copies the entries of one budget into another, moved by <paramref name="dateChange"/> (a date
    /// formula such as "1Y") and multiplied by <paramref name="factor"/>. Returns how many.
    /// </summary>
    public async Task<int> CopyAsync(string fromBudget, string toBudget, string dateChange, decimal factor, bool replace)
    {
        var source = await GetBudgetAsync(fromBudget);
        var target = await GetBudgetAsync(toBudget);
        DateFormula formula = null;
        if (!dateChange.IsNullOrWhiteSpace() && !DateFormula.TryParse(dateChange.Trim().ToUpperInvariant(), out formula))
        {
            throw new BusinessException(ErpErrorCodes.Journals.InvalidDateFormula).WithData("formula", dateChange);
        }

        if (replace)
        {
            await _entryRepository.DeleteAsync(e => e.BudgetName == target.Name);
        }

        var entries = await _entryRepository.GetListAsync(e => e.BudgetName == source.Name);
        foreach (var entry in entries)
        {
            var date = entry.Date.HasValue && formula != null ? formula.Apply(entry.Date.Value) : entry.Date;
            await InsertAsync(target.Name, entry.GLAccountNo, date, Math.Round(entry.Amount * (factor == 0m ? 1m : factor), 2, MidpointRounding.AwayFromZero), entry.Description);
        }

        return entries.Count;
    }

    private async Task InsertAsync(string budgetName, string accountNo, DateTime? date, decimal amount, string description)
    {
        var all = await _entryRepository.GetQueryableAsync();
        var last = await AsyncExecuter.MaxAsync(all.Select(e => (long?)e.EntryNo)) ?? 0L;
        var entry = new GLBudgetEntry(GuidGenerator.Create(), last + 1);
        entry.Set(budgetName, accountNo, date, null, null, amount, description, null, null, null, null, null);
        await _entryRepository.InsertAsync(entry, autoSave: true);
    }
}
