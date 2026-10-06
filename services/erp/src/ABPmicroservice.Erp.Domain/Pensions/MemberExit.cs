using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// Why a member leaves a scheme, and what that entitles the member to: which portions are paid,
/// how much of the employer's money has vested, how the lump sum is taxed and what the member's
/// status becomes. This is where a scheme's rules for withdrawal, retirement and death are set.
/// </summary>
public class ExitReason : CodeTableEntity
{
    public ExitPaymentOption PaymentOption { get; private set; } = ExitPaymentOption.PayEmployeeAndEmployer;

    /// <summary>The share of the employer's money the member has earned the right to, as a percentage.</summary>
    public decimal EmployerPortionPct { get; private set; } = 100m;

    /// <summary>The table the lump sum is taxed by; blank pays it untaxed.</summary>
    public string TaxTableCode { get; private set; }

    /// <summary>Retirement benefits are commonly exempt whatever the tax table says.</summary>
    public bool LumpsumTaxFree { get; private set; }

    /// <summary>The status the exit leaves the member in.</summary>
    public MemberStatus StatusAfterExit { get; private set; } = MemberStatus.Inactive;

    protected ExitReason() { }

    public ExitReason(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(ExitPaymentOption paymentOption, decimal employerPortionPct, string taxTableCode, bool lumpsumTaxFree, MemberStatus statusAfterExit)
    {
        if (employerPortionPct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", employerPortionPct);
        }

        PaymentOption = paymentOption == ExitPaymentOption.None ? ExitPaymentOption.PayEmployeeAndEmployer : paymentOption;
        EmployerPortionPct = employerPortionPct;
        TaxTableCode = NormalizeCode(Check.Length(taxTableCode, nameof(taxTableCode), ErpDomainConsts.MaxCodeLength));
        LumpsumTaxFree = lumpsumTaxFree;
        StatusAfterExit = statusAfterExit is MemberStatus.None or MemberStatus.Active ? MemberStatus.Inactive : statusAfterExit;
    }

    /// <summary>
    /// Vests the employer's money by the sponsor's vesting scale (years of service) instead of the
    /// employer portion above, for sponsors that have a scale. Usual for withdrawals, not for
    /// retirement or death, which vest in full.
    /// </summary>
    public bool ApplyVestingScale { get; private set; }

    public void SetVesting(bool applyVestingScale) => ApplyVestingScale = applyVestingScale;

    /// <summary>A death exit pays the member's beneficiaries, whose shares must then add up to 100%.</summary>
    public bool IsDeath => StatusAfterExit is MemberStatus.DeathInService or MemberStatus.DeathInDeferment or MemberStatus.Deceased;
}

/// <summary>A lump sum tax table: how much of a lump sum is free of tax. Its bands say how the rest is taxed.</summary>
public class LumpsumTaxTable : CodeTableEntity
{
    /// <summary>Tax-free amount earned for each year of service.</summary>
    public decimal AnnualTaxFreeAmount { get; private set; }

    /// <summary>The most that can be tax free however long the service; zero for no cap.</summary>
    public decimal MaxTaxFreeAmount { get; private set; }

    /// <summary>A member older than this pays no tax on the lump sum; zero for no age limit.</summary>
    public int MaxAgeTaxable { get; private set; }

    protected LumpsumTaxTable() { }

    public LumpsumTaxTable(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(decimal annualTaxFreeAmount, decimal maxTaxFreeAmount, int maxAgeTaxable)
    {
        if (annualTaxFreeAmount < 0 || maxTaxFreeAmount < 0 || maxAgeTaxable < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Tax Free Amount");
        }

        AnnualTaxFreeAmount = annualTaxFreeAmount;
        MaxTaxFreeAmount = maxTaxFreeAmount;
        MaxAgeTaxable = maxAgeTaxable;
    }

    public decimal TaxFreeAmount(decimal serviceYears)
    {
        var earned = AnnualTaxFreeAmount * Math.Max(0m, serviceYears);
        return MaxTaxFreeAmount > 0m ? Math.Min(earned, MaxTaxFreeAmount) : earned;
    }
}

/// <summary>One band of a lump sum tax table: the part of the taxable amount between the limits is taxed at the rate.</summary>
public class LumpsumTaxBand : CompanyEntity
{
    public string TaxTableCode { get; private set; }
    public decimal LowerLimit { get; private set; }

    /// <summary>Zero means the band has no upper limit.</summary>
    public decimal UpperLimit { get; private set; }

    public decimal RatePct { get; private set; }

    protected LumpsumTaxBand() { }

    public LumpsumTaxBand(Guid id, string taxTableCode, decimal lowerLimit)
        : base(id)
    {
        SetKey(taxTableCode, lowerLimit);
    }

    public void SetKey(string taxTableCode, decimal lowerLimit)
    {
        TaxTableCode = Check.NotNullOrWhiteSpace(taxTableCode, nameof(taxTableCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        LowerLimit = lowerLimit >= 0 ? lowerLimit : throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Lower Limit");
    }

    public void Set(decimal upperLimit, decimal ratePct)
    {
        if (upperLimit != 0m && upperLimit <= LowerLimit)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.InvalidTaxBand);
        }

        if (ratePct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", ratePct);
        }

        UpperLimit = upperLimit;
        RatePct = ratePct;
    }

    /// <summary>Tax on a taxable amount through progressive bands.</summary>
    public static decimal TaxOn(decimal taxableAmount, IEnumerable<LumpsumTaxBand> bands)
    {
        var tax = 0m;

        foreach (var band in bands.OrderBy(b => b.LowerLimit))
        {
            if (taxableAmount <= band.LowerLimit)
            {
                break;
            }

            var top = band.UpperLimit == 0m ? taxableAmount : Math.Min(taxableAmount, band.UpperLimit);
            tax += (top - band.LowerLimit) * band.RatePct / 100m;
        }

        return Math.Round(tax, 2, MidpointRounding.AwayFromZero);
    }
}

/// <summary>
/// A member's exit from a scheme (a claim): the benefit is calculated from the member's fund as
/// at a date, approved, and posted, which takes the paid portions out of the member's fund and
/// leaves the net amount owing to the member.
/// </summary>
public class MemberExit : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string MemberNo { get; private set; }
    public string MemberName { get; private set; }
    public string SchemeCode { get; private set; }
    public string SponsorNo { get; private set; }
    public string ReasonCode { get; private set; }
    public MemberWithdrawalType WithdrawalType { get; private set; }
    public DateTime ExitDate { get; private set; }

    /// <summary>The benefit counts every entry posted on or before this date.</summary>
    public DateTime DateOfCalculation { get; private set; }

    public MemberExitStatus Status { get; private set; }
    public string Comment { get; private set; }

    public decimal AgeAtExit { get; private set; }
    public decimal ServiceYears { get; private set; }

    public decimal EmployeeBalance { get; private set; }
    public decimal EmployerBalance { get; private set; }
    public decimal EmployeePayable { get; private set; }
    public decimal EmployerPayable { get; private set; }

    /// <summary>What stays in the scheme: the unvested or deferred part of the member's fund.</summary>
    public decimal DeferredAmount { get; private set; }

    public decimal RegisteredPayable { get; private set; }
    public decimal UnregisteredPayable { get; private set; }
    public decimal GrossLumpsum { get; private set; }
    public decimal TaxFreeAmount { get; private set; }
    public decimal TaxableAmount { get; private set; }
    public decimal TaxOnLumpsum { get; private set; }
    public decimal NetPayable { get; private set; }

    public DateTime? PostedDate { get; private set; }
    public string PostedBy { get; private set; }

    protected MemberExit() { }

    public MemberExit(Guid id, string no, PensionMember member, string reasonCode, DateTime exitDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(member, reasonCode, MemberWithdrawalType.Actual, exitDate, exitDate, null);
    }

    public bool IsOpen => Status == MemberExitStatus.Open;

    public void Set(PensionMember member, string reasonCode, MemberWithdrawalType withdrawalType, DateTime exitDate, DateTime? dateOfCalculation, string comment)
    {
        EnsureOpen();
        MemberNo = member.No;
        MemberName = member.FullName;
        SchemeCode = member.SchemeCode;
        SponsorNo = member.SponsorNo;
        ReasonCode = Check.NotNullOrWhiteSpace(reasonCode, nameof(reasonCode), ErpDomainConsts.MaxCodeLength).Trim().ToUpperInvariant();
        WithdrawalType = withdrawalType;
        ExitDate = exitDate.Date;
        DateOfCalculation = (dateOfCalculation ?? exitDate).Date;
        Comment = Check.Length(comment, nameof(comment), ErpDomainConsts.MaxDescriptionLength);
    }

    internal void SetBenefit(
        decimal ageAtExit,
        decimal serviceYears,
        decimal employeeBalance,
        decimal employerBalance,
        decimal employeePayable,
        decimal employerPayable,
        decimal registeredPayable,
        decimal unregisteredPayable,
        decimal taxFreeAmount,
        decimal taxOnLumpsum
    )
    {
        AgeAtExit = ageAtExit;
        ServiceYears = serviceYears;
        EmployeeBalance = employeeBalance;
        EmployerBalance = employerBalance;
        EmployeePayable = employeePayable;
        EmployerPayable = employerPayable;
        DeferredAmount = employeeBalance + employerBalance - employeePayable - employerPayable;
        RegisteredPayable = registeredPayable;
        UnregisteredPayable = unregisteredPayable;
        GrossLumpsum = employeePayable + employerPayable;
        TaxFreeAmount = taxFreeAmount;
        TaxableAmount = Math.Max(0m, registeredPayable - taxFreeAmount);
        TaxOnLumpsum = taxOnLumpsum;
        NetPayable = GrossLumpsum - taxOnLumpsum;
    }

    public void Approve()
    {
        EnsureOpen();
        Status = MemberExitStatus.Approved;
    }

    public void Reopen()
    {
        if (Status is not (MemberExitStatus.Approved or MemberExitStatus.Rejected or MemberExitStatus.Canceled))
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotReleased).WithData("documentNo", No);
        }

        Status = MemberExitStatus.Open;
    }

    public void Cancel()
    {
        if (Status == MemberExitStatus.Posted)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotOpen).WithData("documentNo", No);
        }

        Status = MemberExitStatus.Canceled;
    }

    internal void MarkPosted(DateTime when, string by)
    {
        Status = MemberExitStatus.Posted;
        PostedDate = when;
        PostedBy = by;
    }

    /// <summary>The payment voucher raised to pay the net benefit out of benefits payable.</summary>
    public string PaymentVoucherNo { get; internal set; }

    public void EnsureOpen()
    {
        if (Status != MemberExitStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }
}

/// <summary>
/// Calculates and posts exits of a defined contribution scheme: the benefit is the member's fund,
/// split by the exit reason into what is paid and what is deferred, with tax on the registered
/// part of the lump sum.
/// </summary>
public class MemberExitEngine : DomainService
{
    public const string SourceCode = "PENEXIT";

    private readonly IRepository<MemberExit, Guid> _exits;
    private readonly IRepository<PensionMember, Guid> _members;
    private readonly IRepository<ExitReason, Guid> _reasons;
    private readonly IRepository<LumpsumTaxTable, Guid> _taxTables;
    private readonly IRepository<LumpsumTaxBand, Guid> _taxBands;
    private readonly PensionSetupManager _setupManager;
    private readonly PensionSchemeManager _schemeManager;
    private readonly MemberLedger _memberLedger;
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly SponsorTermsManager _termsManager;
    private readonly PensionBeneficiaryManager _beneficiaryManager;
    private readonly MemberHistoryRecorder _history;
    private readonly ICurrentUser _currentUser;

    public MemberExitEngine(
        IRepository<MemberExit, Guid> exits,
        IRepository<PensionMember, Guid> members,
        IRepository<ExitReason, Guid> reasons,
        IRepository<LumpsumTaxTable, Guid> taxTables,
        IRepository<LumpsumTaxBand, Guid> taxBands,
        PensionSetupManager setupManager,
        PensionSchemeManager schemeManager,
        MemberLedger memberLedger,
        GenJnlPostLine genJnlPostLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager,
        SponsorTermsManager termsManager,
        PensionBeneficiaryManager beneficiaryManager,
        MemberHistoryRecorder history,
        ICurrentUser currentUser
    )
    {
        _termsManager = termsManager;
        _beneficiaryManager = beneficiaryManager;
        _history = history;
        _exits = exits;
        _members = members;
        _reasons = reasons;
        _taxTables = taxTables;
        _taxBands = taxBands;
        _setupManager = setupManager;
        _schemeManager = schemeManager;
        _memberLedger = memberLedger;
        _genJnlPostLine = genJnlPostLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
        _currentUser = currentUser;
    }

    /// <summary>The share of a money type an exit reason pays out, between 0 and 1, at the reason's own employer portion.</summary>
    public static decimal PayableShare(ExitReason reason, MoneyType moneyType) => PayableShare(reason, moneyType, reason.EmployerPortionPct);

    /// <summary>The share of a money type an exit pays out, between 0 and 1, with <paramref name="employerVestedPct"/> of the employer's money vested.</summary>
    public static decimal PayableShare(ExitReason reason, MoneyType moneyType, decimal employerVestedPct)
    {
        if (moneyType.IsEmployee)
        {
            return 1m;
        }

        return reason.PaymentOption == ExitPaymentOption.PayEmployeeAndEmployer ? employerVestedPct / 100m : 0m;
    }

    /// <summary>
    /// How much of the employer's money has vested: by the sponsor's vesting scale for a reason that
    /// uses it and a sponsor that has one, otherwise the reason's employer portion.
    /// </summary>
    public async Task<decimal> GetEmployerVestedPctAsync(ExitReason reason, PensionMember member, decimal serviceYears)
    {
        var scale = reason.ApplyVestingScale ? await _termsManager.GetEmployerVestedPctAsync(member.SponsorNo, serviceYears) : null;
        return scale ?? reason.EmployerPortionPct;
    }

    /// <summary>
    /// Approves the exit on its figures as they stand now. A death benefit is paid to the member's
    /// beneficiaries, so it is approved only once their shares add up to 100%. Every mandatory
    /// document the exit needs must have been received.
    /// </summary>
    public async Task ApproveAsync(MemberExit exit)
    {
        await CalculateAsync(exit);

        var reason = await GetReasonAsync(exit.ReasonCode);
        if (reason.IsDeath && exit.WithdrawalType == MemberWithdrawalType.Actual)
        {
            await _beneficiaryManager.GetCompleteAsync(exit.MemberNo);
        }

        await LazyServiceProvider.LazyGetRequiredService<ExitDocumentManager>().EnsureCompleteAsync(exit);

        exit.Approve();
        await _exits.UpdateAsync(exit, autoSave: true);
    }

    /// <summary>Works the benefit out from the member's fund as at the date of calculation and stores it on the exit.</summary>
    public async Task CalculateAsync(MemberExit exit)
    {
        exit.EnsureOpen();

        var member = await GetMemberAsync(exit.MemberNo);
        var reason = await GetReasonAsync(exit.ReasonCode);
        var balances = await _memberLedger.GetBalancesAsync(member.No, exit.DateOfCalculation);

        var age = YearsBetween(member.DateOfBirth, exit.ExitDate);
        var service = YearsBetween(member.JoinSchemeDate ?? member.DateOfEmployment, exit.ExitDate);
        var vested = await GetEmployerVestedPctAsync(reason, member, service);

        var payable = balances.ByMoneyType.ToDictionary(kv => kv.Key, kv => Round(Math.Max(0m, kv.Value) * PayableShare(reason, kv.Key, vested)));
        var registeredPayable = payable.Where(kv => kv.Key.IsRegistered).Sum(kv => kv.Value);

        var taxFree = 0m;
        var tax = 0m;

        if (!reason.LumpsumTaxFree && !reason.TaxTableCode.IsNullOrWhiteSpace())
        {
            var table = await _taxTables.FirstOrDefaultAsync(t => t.Code == reason.TaxTableCode)
                ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Lumpsum Tax Table").WithData("code", reason.TaxTableCode);

            if (table.MaxAgeTaxable == 0 || age <= table.MaxAgeTaxable)
            {
                taxFree = Math.Min(registeredPayable, table.TaxFreeAmount(service));
                tax = LumpsumTaxBand.TaxOn(registeredPayable - taxFree, await _taxBands.GetListAsync(b => b.TaxTableCode == table.Code));
            }
            else
            {
                taxFree = registeredPayable;
            }
        }
        else
        {
            taxFree = registeredPayable;
        }

        exit.SetBenefit(
            age,
            service,
            balances.Employee,
            balances.Employer,
            payable.Where(kv => kv.Key.IsEmployee).Sum(kv => kv.Value),
            payable.Where(kv => !kv.Key.IsEmployee).Sum(kv => kv.Value),
            registeredPayable,
            payable.Where(kv => !kv.Key.IsRegistered).Sum(kv => kv.Value),
            taxFree,
            tax
        );

        await _exits.UpdateAsync(exit, autoSave: true);
    }

    /// <summary>
    /// Pays the member out: the paid portions leave the member's fund, the member funds liability
    /// is debited with the gross lump sum, the tax withheld is credited to the tax account and the
    /// net to benefits payable, and the member takes the status the exit reason gives.
    /// </summary>
    public async Task PostAsync(MemberExit exit, DateTime postingDate)
    {
        if (exit.Status != MemberExitStatus.Approved)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotReleased).WithData("documentNo", exit.No);
        }

        if (exit.WithdrawalType == MemberWithdrawalType.Projection)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.ProjectionCannotBePosted).WithData("documentNo", exit.No);
        }

        await _glSetupManager.CheckPostingDateAsync(postingDate);

        var member = await GetMemberAsync(exit.MemberNo);
        var reason = await GetReasonAsync(exit.ReasonCode);
        var balances = await _memberLedger.GetBalancesAsync(member.No, exit.DateOfCalculation);

        // The fund is read again: what is posted must be what the member has now, and an exit
        // approved before a later contribution was posted has to be calculated again.
        var vested = await GetEmployerVestedPctAsync(reason, member, YearsBetween(member.JoinSchemeDate ?? member.DateOfEmployment, exit.ExitDate));
        var payable = balances.ByMoneyType.ToDictionary(kv => kv.Key, kv => Round(Math.Max(0m, kv.Value) * PayableShare(reason, kv.Key, vested)));
        var gross = payable.Values.Sum();
        if (gross != exit.GrossLumpsum)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.BenefitOutOfDate).WithData("documentNo", exit.No);
        }

        if (gross == 0m)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NothingToPost).WithData("documentNo", exit.No);
        }

        var setup = await _setupManager.GetAsync();
        var memberFunds = setup.Require(setup.MemberFundsAccountNo, "Member Funds Account No.");
        var benefitsPayable = setup.Require(setup.BenefitsPayableAccountNo, "Benefits Payable Account No.");
        var taxAccount = exit.TaxOnLumpsum == 0m ? null : setup.Require(setup.TaxAccountNo, "Tax Account No.");

        var dimensionSetId = await _schemeManager.GetDimensionSetIdAsync(exit.SchemeCode);
        var register = await _registerManager.OpenAsync(postingDate, SourceCode, exit.No);
        var context = new GLPostingContext(register, SourceCode);
        var description = $"Exit {exit.No} {member.FullName}";

        foreach (var (moneyType, amount) in payable)
        {
            await _memberLedger.PostAsync(
                member, postingDate, null, exit.No, description, PensionTransactionType.Withdrawal, moneyType,
                PensionContributionMode.Normal, -amount, 0m, dimensionSetId, context, _currentUser.UserName
            );
        }

        await _genJnlPostLine.PostGLDirectAsync(memberFunds, postingDate, GLEntryDocumentType.None, exit.No, description, gross, member.No, dimensionSetId, context);
        await _genJnlPostLine.PostGLDirectAsync(benefitsPayable, postingDate, GLEntryDocumentType.None, exit.No, description, -exit.NetPayable, member.No, dimensionSetId, context);

        if (exit.TaxOnLumpsum != 0m)
        {
            await _genJnlPostLine.PostGLDirectAsync(taxAccount, postingDate, GLEntryDocumentType.None, exit.No, "Tax on " + description, -exit.TaxOnLumpsum, member.No, dimensionSetId, context);
        }

        await _registerManager.CloseAsync(register);

        // A member whose money partly stays in the scheme is deferred, whatever the reason says.
        var before = member.Status;
        member.Exit(exit.DeferredAmount > 0m && reason.StatusAfterExit == MemberStatus.Inactive ? MemberStatus.Deferred : reason.StatusAfterExit, exit.ExitDate);
        await _members.UpdateAsync(member);
        await _history.RecordStatusAsync(member, before, exit.ExitDate, exit.No);

        exit.MarkPosted(Clock.Now, _currentUser.UserName);
        await _exits.UpdateAsync(exit, autoSave: true);
    }

    /// <summary>Whole and part years between two dates, to two decimals; zero when the start is unknown.</summary>
    public static decimal YearsBetween(DateTime? from, DateTime to)
    {
        if (!from.HasValue || to <= from.Value)
        {
            return 0m;
        }

        return Math.Round((decimal)(to.Date - from.Value.Date).TotalDays / 365.25m, 2);
    }

    private static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    private async Task<PensionMember> GetMemberAsync(string memberNo)
    {
        return await _members.FirstOrDefaultAsync(m => m.No == memberNo)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Member").WithData("code", memberNo ?? string.Empty);
    }

    private async Task<ExitReason> GetReasonAsync(string reasonCode)
    {
        return await _reasons.FirstOrDefaultAsync(r => r.Code == reasonCode)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Exit Reason").WithData("code", reasonCode ?? string.Empty);
    }
}
