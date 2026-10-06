using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// One change in a pensioner's history: an increment, a suspension, a reinstatement, a life
/// certificate received or arrears paid. It shows the monthly pension before and after the change.
/// </summary>
public class PensionerChangeEntry : CompanyEntity
{
    public string PensionerNo { get; private set; }
    public string SchemeCode { get; private set; }
    public PensionerChangeType ChangeType { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public decimal OldMonthlyPension { get; private set; }
    public decimal NewMonthlyPension { get; internal set; }

    /// <summary>The money the change involves: arrears raised or paid.</summary>
    public decimal Amount { get; internal set; }

    public string Description { get; internal set; }
    public string DocumentNo { get; private set; }
    public string UserName { get; private set; }

    protected PensionerChangeEntry() { }

    public PensionerChangeEntry(Guid id, Pensioner pensioner, PensionerChangeType changeType, DateTime effectiveDate, string documentNo, string userName)
        : base(id)
    {
        PensionerNo = pensioner.No;
        SchemeCode = pensioner.SchemeCode;
        ChangeType = changeType;
        EffectiveDate = effectiveDate.Date;
        OldMonthlyPension = pensioner.MonthlyPension;
        NewMonthlyPension = pensioner.MonthlyPension;
        DocumentNo = documentNo;
        UserName = userName;
    }
}

/// <summary>
/// A pension increment: a scheme raises the pensions in payment by a percentage from a date, with a
/// floor below which no pension is left. Applying it changes every pension at once and, when the date
/// is in the past, owes the pensioners the difference for the months already paid at the old rate.
/// </summary>
public class PensionIncrement : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string SchemeCode { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public decimal IncrementPct { get; private set; }

    /// <summary>No pension is left below this after the increment; zero sets no floor.</summary>
    public decimal MinimumMonthlyPension { get; private set; }

    public string Description { get; private set; }

    /// <summary>Why the pensions are revised.</summary>
    public string ReasonCode { get; private set; }

    public PensionIncrementStatus Status { get; private set; }
    public DateTime? AppliedDate { get; private set; }
    public string AppliedBy { get; private set; }
    public int NoOfPensioners { get; private set; }
    public decimal TotalMonthlyIncrease { get; private set; }
    public decimal TotalArrears { get; private set; }

    protected PensionIncrement() { }

    public PensionIncrement(Guid id, string no, string schemeCode, DateTime effectiveDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(schemeCode, effectiveDate, 0m, 0m, null);
    }

    public void Set(string schemeCode, DateTime effectiveDate, decimal incrementPct, decimal minimumMonthlyPension, string description)
    {
        EnsureOpen();
        if (incrementPct is < -100 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", incrementPct);
        }

        if (minimumMonthlyPension < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Minimum Monthly Pension");
        }

        SchemeCode = PensionSponsor.SchemeOf(schemeCode);
        EffectiveDate = new DateTime(effectiveDate.Year, effectiveDate.Month, 1);
        IncrementPct = incrementPct;
        MinimumMonthlyPension = minimumMonthlyPension;
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    public void SetReason(string reasonCode)
    {
        EnsureOpen();
        ReasonCode = CodeTableEntity.NormalizeCode(Check.Length(reasonCode, nameof(reasonCode), ErpDomainConsts.MaxCodeLength));
    }

    /// <summary>The pension a pensioner gets after the increment.</summary>
    public decimal NewPension(decimal monthlyPension)
    {
        var raised = Math.Round(monthlyPension * (1m + IncrementPct / 100m), 2, MidpointRounding.AwayFromZero);
        return Math.Max(raised, MinimumMonthlyPension);
    }

    internal void MarkApplied(int noOfPensioners, decimal totalMonthlyIncrease, decimal totalArrears, DateTime when, string by)
    {
        Status = PensionIncrementStatus.Applied;
        NoOfPensioners = noOfPensioners;
        TotalMonthlyIncrease = totalMonthlyIncrease;
        TotalArrears = totalArrears;
        AppliedDate = when;
        AppliedBy = by;
    }

    public void EnsureOpen()
    {
        if (Status != PensionIncrementStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.IncrementAlreadyApplied).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>
/// Looks after pensions in payment: increments, suspensions and reinstatements, and the life
/// certificates that keep a pension paid. Every change is kept in the pensioner's history.
/// </summary>
public class PensionerAdministrator : DomainService
{
    /// <summary>The reason a pension stopped for want of a life certificate; receiving one starts it again.</summary>
    public const string LifeCertificateOverdue = "Life certificate not received";

    private readonly IRepository<Pensioner, Guid> _pensioners;
    private readonly IRepository<PensionIncrement, Guid> _increments;
    private readonly IRepository<PensionerChangeEntry, Guid> _changes;
    private readonly IRepository<PensionerSuspensionReason, Guid> _suspensionReasons;
    private readonly PensionSetupManager _setupManager;
    private readonly ICurrentUser _currentUser;

    public PensionerAdministrator(
        IRepository<Pensioner, Guid> pensioners,
        IRepository<PensionIncrement, Guid> increments,
        IRepository<PensionerChangeEntry, Guid> changes,
        IRepository<PensionerSuspensionReason, Guid> suspensionReasons,
        PensionSetupManager setupManager,
        ICurrentUser currentUser
    )
    {
        _suspensionReasons = suspensionReasons;
        _pensioners = pensioners;
        _increments = increments;
        _changes = changes;
        _setupManager = setupManager;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Raises every pension of the scheme that is in payment (active or suspended) on the effective
    /// date. Months from the effective date that a payroll has already paid at the old pension are
    /// owed as arrears, paid with the next payroll.
    /// </summary>
    public async Task ApplyIncrementAsync(PensionIncrement increment)
    {
        increment.EnsureOpen();

        var scheme = increment.SchemeCode;
        var effective = increment.EffectiveDate;
        var pensioners = (await _pensioners.GetListAsync(p => p.SchemeCode == scheme && p.Status != PensionerStatus.Ceased && p.StartDate <= effective))
            .OrderBy(p => p.No, StringComparer.Ordinal)
            .ToList();

        var count = 0;
        var totalIncrease = 0m;
        var totalArrears = 0m;

        foreach (var pensioner in pensioners)
        {
            var old = pensioner.MonthlyPension;
            var raised = increment.NewPension(old);
            if (raised == old)
            {
                continue;
            }

            var paidMonths = pensioner.LastPaidPeriod >= effective
                ? (pensioner.LastPaidPeriod.Value.Year - effective.Year) * 12 + pensioner.LastPaidPeriod.Value.Month - effective.Month + 1
                : 0;
            var arrears = Math.Max(0m, (raised - old) * paidMonths);

            var change = new PensionerChangeEntry(GuidGenerator.Create(), pensioner, PensionerChangeType.Increment, effective, increment.No, _currentUser.UserName)
            {
                NewMonthlyPension = raised,
                Amount = arrears,
                Description = increment.Description.IsNullOrWhiteSpace() ? $"Increment of {increment.IncrementPct}%" : increment.Description,
            };

            pensioner.ChangePension(raised);
            if (arrears > 0m)
            {
                pensioner.AddArrears(arrears, paidMonths);
            }

            await _pensioners.UpdateAsync(pensioner);
            await _changes.InsertAsync(change);

            count++;
            totalIncrease += raised - old;
            totalArrears += arrears;
        }

        increment.MarkApplied(count, totalIncrease, totalArrears, Clock.Now, _currentUser.UserName);
        await _increments.UpdateAsync(increment, autoSave: true);
    }

    /// <summary>
    /// Stops the pension for a suspension reason, a reason in words, or both: the words are added to
    /// the reason's description.
    /// </summary>
    public async Task SuspendAsync(Pensioner pensioner, string reasonCode, string reason, DateTime date)
    {
        var code = CodeTableEntity.NormalizeCode(reasonCode);
        var setReason = code == null ? null : await _suspensionReasons.FirstOrDefaultAsync(r => r.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pensioner Suspension Reason").WithData("code", code);

        var text = string.Join(": ", new[] { setReason?.Description ?? setReason?.Code, reason?.Trim() }.Where(t => !t.IsNullOrWhiteSpace()));
        pensioner.Suspend(code, text.IsNullOrWhiteSpace() ? "Suspended" : text);
        await _pensioners.UpdateAsync(pensioner);
        await _changes.InsertAsync(
            new PensionerChangeEntry(GuidGenerator.Create(), pensioner, PensionerChangeType.Suspension, date, null, _currentUser.UserName) { Description = pensioner.SuspensionReason },
            autoSave: true
        );
    }

    /// <summary>
    /// Pays a suspended pension again. The months it was due but not paid, up to the month of
    /// <paramref name="date"/>, become arrears paid with the next payroll.
    /// </summary>
    public async Task ReinstateAsync(Pensioner pensioner, DateTime date)
    {
        var reason = pensioner.SuspensionReason;
        pensioner.Reinstate();

        var months = pensioner.UnpaidMonthsBefore(date);
        var arrears = months * pensioner.MonthlyPension;
        if (arrears > 0m)
        {
            pensioner.AddArrears(arrears, months);
        }

        await _pensioners.UpdateAsync(pensioner);
        await _changes.InsertAsync(
            new PensionerChangeEntry(GuidGenerator.Create(), pensioner, PensionerChangeType.Reinstatement, date, null, _currentUser.UserName)
            {
                Amount = arrears,
                Description = months == 0 ? $"Reinstated after: {reason}" : $"Reinstated after: {reason}. Arrears for {months} month(s).",
            },
            autoSave: true
        );
    }

    /// <summary>Records proof that the pensioner is alive; a pension stopped for want of it is paid again.</summary>
    public async Task RecordLifeCertificateAsync(Pensioner pensioner, DateTime date)
    {
        var setup = await _setupManager.GetAsync();
        pensioner.RecordLifeCertificate(date, setup.LifeCertificateFrequencyMonths);
        await _changes.InsertAsync(
            new PensionerChangeEntry(GuidGenerator.Create(), pensioner, PensionerChangeType.LifeCertificate, date, null, _currentUser.UserName)
            {
                Description = pensioner.LifeCertificateDueDate.HasValue ? $"Next due {pensioner.LifeCertificateDueDate:yyyy-MM-dd}" : null,
            }
        );

        if (pensioner.Status == PensionerStatus.Suspended && await IsForLifeCertificateAsync(pensioner))
        {
            await ReinstateAsync(pensioner, date);
        }
        else
        {
            await _pensioners.UpdateAsync(pensioner, autoSave: true);
        }
    }

    /// <summary>
    /// Suspends every active pensioner of the scheme whose life certificate was due before
    /// <paramref name="asOf"/>. A pensioner who has never sent one is due a certificate period after the
    /// pension started. Returns how many were suspended.
    /// </summary>
    public async Task<int> SuspendOverdueAsync(string schemeCode, DateTime asOf)
    {
        var setup = await _setupManager.GetAsync();
        if (setup.LifeCertificateFrequencyMonths <= 0)
        {
            return 0;
        }

        var scheme = PensionSponsor.SchemeOf(schemeCode);
        var date = asOf.Date;
        var overdue = (await _pensioners.GetListAsync(p => p.SchemeCode == scheme && p.Status == PensionerStatus.Active))
            .Where(p => (p.LifeCertificateDueDate ?? p.StartDate.AddMonths(setup.LifeCertificateFrequencyMonths)) < date)
            .ToList();

        // The suspension reason set up for missing certificates, when there is one.
        var reason = (await _suspensionReasons.GetListAsync(r => r.LifeCertificate)).OrderBy(r => r.Code, StringComparer.Ordinal).FirstOrDefault();
        foreach (var pensioner in overdue)
        {
            await SuspendAsync(pensioner, reason?.Code, reason == null ? LifeCertificateOverdue : null, date);
        }

        return overdue.Count;
    }

    /// <summary>Whether the pension stopped for want of a life certificate.</summary>
    private async Task<bool> IsForLifeCertificateAsync(Pensioner pensioner)
    {
        if (pensioner.SuspensionReason == LifeCertificateOverdue)
        {
            return true;
        }

        var code = pensioner.SuspensionReasonCode;
        return code != null && await _suspensionReasons.AnyAsync(r => r.Code == code && r.LifeCertificate);
    }
}
