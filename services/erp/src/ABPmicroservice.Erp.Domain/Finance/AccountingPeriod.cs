using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Accounting Period. Mirrors Business Central table 50: the first day of a period, and whether it
/// opens a fiscal year. A period runs until the next one starts.
/// </summary>
public class AccountingPeriod : CompanyEntity
{
    public DateTime StartingDate { get; private set; }
    public string Name { get; private set; }
    public bool NewFiscalYear { get; private set; }

    /// <summary>Set by Close Year on every period of a closed fiscal year.</summary>
    public bool Closed { get; private set; }

    /// <summary>Set with Closed; BC's "Date Locked" stops the period from being edited.</summary>
    public bool DateLocked { get; private set; }

    protected AccountingPeriod() { }

    public AccountingPeriod(Guid id, DateTime startingDate, string name, bool newFiscalYear)
        : base(id)
    {
        StartingDate = startingDate.Date;
        Name = Check.Length(name, nameof(name), ErpDomainConsts.MaxAccountingPeriodNameLength);
        NewFiscalYear = newFiscalYear;
    }

    internal void Close()
    {
        Closed = true;
        DateLocked = true;
    }
}

/// <summary>Create Year and Close Year of Business Central's Accounting Periods page.</summary>
public class AccountingPeriodManager : DomainService
{
    private readonly IRepository<AccountingPeriod, Guid> _repository;

    public AccountingPeriodManager(IRepository<AccountingPeriod, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Creates a fiscal year of <paramref name="noOfPeriods"/> periods, each <paramref name="periodLength"/>
    /// long (a date formula, "1M" by default), from <paramref name="startingDate"/>, plus the first
    /// period of the next year so the year has an end (BC codeunit "Create Fiscal Year").
    /// </summary>
    public async Task<List<AccountingPeriod>> CreateFiscalYearAsync(DateTime startingDate, int noOfPeriods, string periodLength)
    {
        if (noOfPeriods < 1 || noOfPeriods > 366)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidPeriodCount);
        }

        var formulaText = periodLength.IsNullOrWhiteSpace() ? "1M" : periodLength.Trim().ToUpperInvariant();
        if (!DateFormula.TryParse(formulaText, out var formula))
        {
            throw new BusinessException(ErpErrorCodes.Journals.InvalidDateFormula).WithData("formula", formulaText);
        }

        var dates = new List<DateTime>();
        var date = startingDate.Date;
        for (var i = 0; i <= noOfPeriods; i++)
        {
            dates.Add(date);
            date = formula.Apply(date);
        }

        // The previous Create Year already made the first period of this year, and this one makes
        // the first period of the next; either may be there already and is then kept. Any other
        // existing period means the year overlaps one that exists.
        var existing = await _repository.GetListAsync(p => dates.Contains(p.StartingDate));
        var reusable = existing
            .Where(p => (p.StartingDate == dates[0] && p.NewFiscalYear && !p.Closed) || p.StartingDate == dates[^1])
            .Select(p => p.StartingDate)
            .ToHashSet();
        var clash = existing.FirstOrDefault(p => !reusable.Contains(p.StartingDate));
        var (yearStart, yearEnd) = (dates[0], dates[^1]);
        if (clash != null || await _repository.AnyAsync(p => p.StartingDate > yearStart && p.StartingDate < yearEnd))
        {
            var clashDate = clash?.StartingDate ?? yearStart;
            throw new BusinessException(ErpErrorCodes.GeneralLedger.AccountingPeriodExists)
                .WithData("startingDate", clashDate.ToString("yyyy-MM-dd"));
        }

        var created = new List<AccountingPeriod>();
        for (var i = 0; i < dates.Count; i++)
        {
            if (reusable.Contains(dates[i]))
            {
                continue;
            }

            var period = new AccountingPeriod(
                GuidGenerator.Create(),
                dates[i],
                dates[i].ToString("MMMM", CultureInfo.InvariantCulture),
                newFiscalYear: i == 0 || i == dates.Count - 1
            );
            created.Add(period);
        }

        await _repository.InsertManyAsync(created, autoSave: true);
        return created;
    }

    /// <summary>
    /// Closes the oldest open fiscal year (BC "Close Year"). The following year must already
    /// exist, so the closed year has an end date.
    /// </summary>
    public async Task<(DateTime From, DateTime To)> CloseFiscalYearAsync()
    {
        var periods = (await _repository.GetListAsync()).OrderBy(p => p.StartingDate).ToList();
        var yearStart = periods.FirstOrDefault(p => p.NewFiscalYear && !p.Closed)
            ?? throw new BusinessException(ErpErrorCodes.GeneralLedger.NoOpenFiscalYear);

        var nextYear = periods.FirstOrDefault(p => p.NewFiscalYear && p.StartingDate > yearStart.StartingDate)
            ?? throw new BusinessException(ErpErrorCodes.GeneralLedger.FiscalYearNotComplete);

        var yearPeriods = periods.Where(p => p.StartingDate >= yearStart.StartingDate && p.StartingDate < nextYear.StartingDate).ToList();
        yearPeriods.ForEach(p => p.Close());
        await _repository.UpdateManyAsync(yearPeriods, autoSave: true);

        return (yearStart.StartingDate, nextYear.StartingDate.AddDays(-1));
    }
}
