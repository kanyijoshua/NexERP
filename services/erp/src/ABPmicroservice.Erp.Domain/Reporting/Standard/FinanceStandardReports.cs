using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>G/L Register: each posting run and the entries it created.</summary>
public class GLRegisterReport : StandardReportBase
{
    private readonly IRepository<GLRegister, Guid> _registers;

    public GLRegisterReport(IRepository<GLRegister, Guid> registers)
    {
        _registers = registers;
    }

    public override StandardReportDefinition Definition { get; } =
        new(3, "GLRegister", "G/L Register", StandardReportAreas.Finance, StandardReportParameters.Period);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Date(result, "postingDate", "Posting Date");
        Text(result, "sourceCode", "Source Code");
        Text(result, "journalBatchName", "Journal Batch Name");
        Text(result, "userName", "User ID");
        Number(result, "fromEntryNo", "From Entry No.");
        Number(result, "toEntryNo", "To Entry No.");
        Text(result, "reversed", "Reversed");

        var registers = await _registers.GetListAsync(r => r.PostingDate >= request.From && r.PostingDate <= request.To);
        foreach (var register in registers.OrderBy(r => r.No))
        {
            Row(
                result,
                ("no", register.No.ToString()),
                ("postingDate", register.PostingDate),
                ("sourceCode", register.SourceCode),
                ("journalBatchName", register.JournalBatchName),
                ("userName", register.UserName),
                ("fromEntryNo", register.FromEntryNo),
                ("toEntryNo", register.ToEntryNo),
                ("reversed", register.Reversed ? "Yes" : string.Empty)
            );
        }

        return result;
    }
}

/// <summary>
/// Trial Balance/Budget: each posting account's movement in the
/// period beside what the chosen budget allowed for it, and how much of the budget that is.
/// </summary>
public class TrialBalanceBudgetReport : StandardReportBase
{
    private readonly IRepository<GLAccount, Guid> _accounts;
    private readonly IRepository<GLEntry, Guid> _entries;
    private readonly IRepository<GLBudgetEntry, Guid> _budgetEntries;

    public TrialBalanceBudgetReport(
        IRepository<GLAccount, Guid> accounts,
        IRepository<GLEntry, Guid> entries,
        IRepository<GLBudgetEntry, Guid> budgetEntries
    )
    {
        _accounts = accounts;
        _entries = entries;
        _budgetEntries = budgetEntries;
    }

    public override StandardReportDefinition Definition { get; } =
        new(
            9,
            "TrialBalanceBudget",
            "Trial Balance/Budget",
            StandardReportAreas.Finance,
            StandardReportParameters.Period | StandardReportParameters.NoFilter | StandardReportParameters.BudgetName
        );

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "accountNo", "No.");
        Text(result, "name", "Name");
        Number(result, "netChange", "Net Change");
        Number(result, "budget", "Budget");
        Number(result, "variance", "Variance");
        Number(result, "percentOfBudget", "% of Budget");

        var filter = Filter(request);
        var budgetName = request.BudgetName?.Trim().ToUpperInvariant();

        var actual = (await _entries.GetListAsync(e => e.PostingDate >= request.From && e.PostingDate <= request.To))
            .GroupBy(e => e.GLAccountNo)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount), StringComparer.Ordinal);

        var budget = (await _budgetEntries.GetListAsync(e =>
                e.Date >= request.From && e.Date <= request.To && (budgetName == null || budgetName == "" || e.BudgetName == budgetName)
            ))
            .GroupBy(e => e.GLAccountNo)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount), StringComparer.Ordinal);

        var totalActual = 0m;
        var totalBudget = 0m;

        foreach (var account in (await _accounts.GetListAsync()).OrderBy(a => a.No, StringComparer.Ordinal))
        {
            if (!filter.Matches(account.No))
            {
                continue;
            }

            var netChange = actual.GetValueOrDefault(account.No);
            var budgeted = budget.GetValueOrDefault(account.No);
            if (netChange == 0m && budgeted == 0m)
            {
                continue;
            }

            totalActual += netChange;
            totalBudget += budgeted;

            var row = Row(
                result,
                ("accountNo", account.No),
                ("name", account.Name),
                ("netChange", netChange),
                ("budget", budgeted),
                ("variance", netChange - budgeted),
                ("percentOfBudget", PercentOf(netChange, budgeted))
            );
            row.DrillDownFilter = account.No;
        }

        BoldRow(
            result,
            ("accountNo", string.Empty),
            ("name", "Total"),
            ("netChange", totalActual),
            ("budget", totalBudget),
            ("variance", totalActual - totalBudget),
            ("percentOfBudget", PercentOf(totalActual, totalBudget))
        );

        return result;
    }

    private static decimal PercentOf(decimal amount, decimal budget) => budget == 0m ? 0m : Math.Round(amount / budget * 100m, 1);
}

/// <summary>Budget: the budgeted amount of each account, month by month.</summary>
public class BudgetReport : StandardReportBase
{
    private readonly IRepository<GLAccount, Guid> _accounts;
    private readonly IRepository<GLBudgetEntry, Guid> _budgetEntries;

    public BudgetReport(IRepository<GLAccount, Guid> accounts, IRepository<GLBudgetEntry, Guid> budgetEntries)
    {
        _accounts = accounts;
        _budgetEntries = budgetEntries;
    }

    public override StandardReportDefinition Definition { get; } =
        new(
            8,
            "Budget",
            "Budget",
            StandardReportAreas.Finance,
            StandardReportParameters.Period | StandardReportParameters.NoFilter | StandardReportParameters.BudgetName
        );

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        var filter = Filter(request);
        var budgetName = request.BudgetName?.Trim().ToUpperInvariant();

        var entries = (await _budgetEntries.GetListAsync(e =>
                e.Date >= request.From && e.Date <= request.To && (budgetName == null || budgetName == "" || e.BudgetName == budgetName)
            ))
            .Where(e => e.Date.HasValue && filter.Matches(e.GLAccountNo))
            .ToList();

        var months = entries
            .Select(e => new DateTime(e.Date.Value.Year, e.Date.Value.Month, 1))
            .Distinct()
            .OrderBy(m => m)
            .ToList();

        Text(result, "accountNo", "No.");
        Text(result, "name", "Name");
        foreach (var month in months)
        {
            Number(result, MonthKey(month), month.ToString("MMM yyyy"));
        }

        Number(result, "total", "Total");

        var names = (await _accounts.GetListAsync()).ToDictionary(a => a.No, a => a.Name, StringComparer.Ordinal);

        foreach (var account in entries.GroupBy(e => e.GLAccountNo).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var row = Row(result, ("accountNo", account.Key), ("name", names.GetValueOrDefault(account.Key, string.Empty)));
            foreach (var month in months)
            {
                row.Values[MonthKey(month)] = account
                    .Where(e => e.Date.Value.Year == month.Year && e.Date.Value.Month == month.Month)
                    .Sum(e => e.Amount);
            }

            row.Values["total"] = account.Sum(e => e.Amount);
        }

        var total = BoldRow(result, ("accountNo", string.Empty), ("name", "Total"), ("total", entries.Sum(e => e.Amount)));
        foreach (var month in months)
        {
            total.Values[MonthKey(month)] = entries
                .Where(e => e.Date.Value.Year == month.Year && e.Date.Value.Month == month.Month)
                .Sum(e => e.Amount);
        }

        return result;
    }

    private static string MonthKey(DateTime month) => $"m{month:yyyyMM}";
}

/// <summary>
/// Bank Acc. - Detail Trial Bal.: each bank account's balance
/// at the start of the period, its entries in the period and the running balance after each.
/// </summary>
public class BankAccountDetailTrialBalanceReport : StandardReportBase
{
    private readonly IRepository<BankAccount, Guid> _bankAccounts;
    private readonly IRepository<BankAccountLedgerEntry, Guid> _entries;

    public BankAccountDetailTrialBalanceReport(
        IRepository<BankAccount, Guid> bankAccounts,
        IRepository<BankAccountLedgerEntry, Guid> entries
    )
    {
        _bankAccounts = bankAccounts;
        _entries = entries;
    }

    public override StandardReportDefinition Definition { get; } =
        new(
            1404,
            "BankAccountDetailTrialBalance",
            "Bank Acc. - Detail Trial Bal.",
            StandardReportAreas.CashManagement,
            StandardReportParameters.Period | StandardReportParameters.NoFilter
        );

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Date(result, "postingDate", "Posting Date");
        Text(result, "documentNo", "Document No.");
        Text(result, "description", "Description");
        Number(result, "debit", "Debit");
        Number(result, "credit", "Credit");
        Number(result, "balance", "Balance");

        var filter = Filter(request);
        var entries = (await _entries.GetListAsync(e => e.PostingDate <= request.To)).ToLookup(e => e.BankAccountNo);

        foreach (var account in (await _bankAccounts.GetListAsync()).OrderBy(a => a.No, StringComparer.Ordinal))
        {
            if (!filter.Matches(account.No))
            {
                continue;
            }

            var balance = entries[account.No].Where(e => e.PostingDate < request.From).Sum(e => e.Amount);
            var inPeriod = entries[account.No].Where(e => e.PostingDate >= request.From).OrderBy(e => e.PostingDate).ThenBy(e => e.EntryNo).ToList();
            if (balance == 0m && inPeriod.Count == 0)
            {
                continue;
            }

            BoldRow(result, ("documentNo", account.No), ("description", account.Name), ("balance", balance));

            foreach (var entry in inPeriod)
            {
                balance += entry.Amount;
                var row = Row(
                    result,
                    ("postingDate", entry.PostingDate),
                    ("documentNo", entry.DocumentNo),
                    ("description", entry.Description),
                    ("debit", Debit(entry.Amount)),
                    ("credit", Credit(entry.Amount)),
                    ("balance", balance)
                );
                row.Indentation = 1;
                row.Italic = entry.Reversed;
            }
        }

        return result;
    }
}

/// <summary>Bank Account - Check Details: the cheques issued from each bank account.</summary>
public class BankAccountCheckDetailsReport : StandardReportBase
{
    private readonly IRepository<CheckLedgerEntry, Guid> _checks;

    public BankAccountCheckDetailsReport(IRepository<CheckLedgerEntry, Guid> checks)
    {
        _checks = checks;
    }

    public override StandardReportDefinition Definition { get; } =
        new(
            1406,
            "BankAccountCheckDetails",
            "Bank Account - Check Details",
            StandardReportAreas.CashManagement,
            StandardReportParameters.Period | StandardReportParameters.NoFilter
        );

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "bankAccountNo", "Bank Account No.");
        Date(result, "checkDate", "Check Date");
        Text(result, "checkNo", "Check No.");
        Text(result, "description", "Description");
        Text(result, "entryStatus", "Entry Status");
        Number(result, "amount", "Amount");

        var filter = Filter(request);
        var checks = (await _checks.GetListAsync(c => c.CheckDate >= request.From && c.CheckDate <= request.To))
            .Where(c => filter.Matches(c.BankAccountNo ?? string.Empty))
            .OrderBy(c => c.BankAccountNo, StringComparer.Ordinal)
            .ThenBy(c => c.CheckDate)
            .ThenBy(c => c.CheckNo, StringComparer.Ordinal)
            .ToList();

        foreach (var check in checks)
        {
            Row(
                result,
                ("bankAccountNo", check.BankAccountNo),
                ("checkDate", check.CheckDate),
                ("checkNo", check.CheckNo),
                ("description", check.Description),
                ("entryStatus", check.EntryStatus.ToString()),
                ("amount", check.Amount)
            );
        }

        BoldRow(result, ("description", "Total"), ("amount", checks.Sum(c => c.Amount)));
        return result;
    }
}

/// <summary>Bank Account Statement: the lines of the posted bank statements.</summary>
public class BankAccountStatementReport : StandardReportBase
{
    private readonly IRepository<BankAccountStatement, Guid> _statements;
    private readonly IRepository<BankAccountStatementLine, Guid> _lines;

    public BankAccountStatementReport(
        IRepository<BankAccountStatement, Guid> statements,
        IRepository<BankAccountStatementLine, Guid> lines
    )
    {
        _statements = statements;
        _lines = lines;
    }

    public override StandardReportDefinition Definition { get; } =
        new(
            1407,
            "BankAccountStatement",
            "Bank Account Statement",
            StandardReportAreas.CashManagement,
            StandardReportParameters.Period | StandardReportParameters.NoFilter
        );

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Date(result, "transactionDate", "Transaction Date");
        Text(result, "documentNo", "Document No.");
        Text(result, "description", "Description");
        Number(result, "statementAmount", "Statement Amount");
        Number(result, "appliedAmount", "Applied Amount");
        Number(result, "difference", "Difference");

        var filter = Filter(request);
        var statements = (await _statements.GetListAsync(s => s.StatementDate >= request.From && s.StatementDate <= request.To))
            .Where(s => filter.Matches(s.BankAccountNo ?? string.Empty))
            .OrderBy(s => s.BankAccountNo, StringComparer.Ordinal)
            .ThenBy(s => s.StatementNo, StringComparer.Ordinal)
            .ToList();

        var lines = (await _lines.GetListAsync()).ToLookup(l => (l.BankAccountNo, l.StatementNo));

        foreach (var statement in statements)
        {
            BoldRow(
                result,
                ("transactionDate", statement.StatementDate),
                ("documentNo", $"{statement.BankAccountNo} / {statement.StatementNo}"),
                ("description", "Balance Last Statement"),
                ("statementAmount", statement.BalanceLastStatement)
            );

            foreach (var line in lines[(statement.BankAccountNo, statement.StatementNo)].OrderBy(l => l.StatementLineNo))
            {
                var row = Row(
                    result,
                    ("transactionDate", line.TransactionDate),
                    ("documentNo", line.DocumentNo),
                    ("description", line.Description),
                    ("statementAmount", line.StatementAmount),
                    ("appliedAmount", line.AppliedAmount),
                    ("difference", line.Difference)
                );
                row.Indentation = 1;
            }

            BoldRow(result, ("description", "Statement Ending Balance"), ("statementAmount", statement.StatementEndingBalance));
        }

        return result;
    }
}
