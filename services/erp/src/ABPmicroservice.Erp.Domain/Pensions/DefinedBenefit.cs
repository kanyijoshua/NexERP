using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// The pension a member of a defined benefit scheme retires on, worked out by the scheme's formula:
/// <c>final pensionable salary × years of pensionable service × accrual rate</c>, cut for each year
/// the member retires early, with part of it given up for a lump sum if the member chooses.
/// <para>
/// It is calculated (Open) and approved. Approving it makes the member a pensioner on the monthly
/// pension and raises a payment voucher for the lump sum.
/// </para>
/// </summary>
public class PensionBenefitCalculation : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string MemberNo { get; private set; }
    public string MemberName { get; private set; }
    public string SchemeCode { get; private set; }
    public DateTime CalculationDate { get; private set; }
    public DateTime RetirementDate { get; private set; }

    /// <summary>Annual salary the pension is based on; zero takes twelve times the member's current monthly salary.</summary>
    public decimal FinalPensionableSalary { get; private set; }

    /// <summary>The share of the annual pension the member gives up for a lump sum.</summary>
    public decimal CommutationPct { get; private set; }

    public string Comment { get; private set; }

    public decimal AgeAtRetirement { get; private set; }
    public decimal PensionableServiceYears { get; private set; }

    /// <summary>The scheme's formula as it stood when the pension was worked out.</summary>
    public decimal AccrualRatePct { get; private set; }

    public decimal CommutationFactor { get; private set; }
    public decimal EarlyReductionPct { get; private set; }

    /// <summary>What the formula gives before any early retirement cut.</summary>
    public decimal FullAnnualPension { get; private set; }

    /// <summary>After the early retirement cut, before commutation.</summary>
    public decimal ReducedAnnualPension { get; private set; }

    public decimal CommutedAnnualPension { get; private set; }
    public decimal LumpSum { get; private set; }

    /// <summary>What is paid each year after commutation, and each month.</summary>
    public decimal AnnualPension { get; private set; }

    public decimal MonthlyPension { get; private set; }

    public BenefitCalculationStatus Status { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public string ApprovedBy { get; private set; }

    /// <summary>The pensioner record approval created, and the voucher raised for the lump sum.</summary>
    public string PensionerNo { get; private set; }

    public string PaymentVoucherNo { get; private set; }

    protected PensionBenefitCalculation() { }

    public PensionBenefitCalculation(Guid id, string no, PensionMember member, DateTime retirementDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength).Trim().ToUpperInvariant();
        Set(member, retirementDate, retirementDate, 0m, 0m, null);
    }

    public bool IsOpen => Status == BenefitCalculationStatus.Open;

    public void Set(PensionMember member, DateTime calculationDate, DateTime retirementDate, decimal finalPensionableSalary, decimal commutationPct, string comment)
    {
        EnsureOpen();
        if (finalPensionableSalary < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Final Pensionable Salary");
        }

        if (commutationPct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", commutationPct);
        }

        MemberNo = member.No;
        MemberName = member.FullName;
        SchemeCode = member.SchemeCode;
        CalculationDate = calculationDate.Date;
        RetirementDate = retirementDate.Date;
        FinalPensionableSalary = finalPensionableSalary;
        CommutationPct = commutationPct;
        Comment = Check.Length(comment, nameof(comment), ErpDomainConsts.MaxDescriptionLength);
    }

    /// <summary>Years between two days, to two decimals, counting a year as 365.25 days.</summary>
    public static decimal YearsBetween(DateTime from, DateTime to) =>
        Math.Max(0m, Math.Round((decimal)(to.Date - from.Date).TotalDays / 365.25m, 2, MidpointRounding.AwayFromZero));

    /// <summary>Works the pension out from the member and the scheme's formula.</summary>
    internal void Calculate(PensionMember member, PensionScheme scheme)
    {
        EnsureOpen();

        if (!scheme.PaysDefinedBenefit)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NotDefinedBenefitScheme).WithData("scheme", scheme.Code);
        }

        if (!member.DateOfBirth.HasValue)
        {
            throw MissingData(member, "Date of Birth");
        }

        var joined = member.JoinSchemeDate ?? member.DateOfEmployment ?? throw MissingData(member, "Join Scheme Date");
        var salary = FinalPensionableSalary > 0m ? FinalPensionableSalary : member.CurrentSalary * 12m;
        if (salary <= 0m)
        {
            throw MissingData(member, "Current Salary");
        }

        AgeAtRetirement = YearsBetween(member.DateOfBirth.Value, RetirementDate);
        if (AgeAtRetirement < scheme.MinimumRetirementAge)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.RetirementTooEarly)
                .WithData("memberNo", member.No)
                .WithData("age", AgeAtRetirement.ToString("0.##"))
                .WithData("minimum", scheme.MinimumRetirementAge);
        }

        if (CommutationPct > scheme.MaxCommutationPct)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.CommutationAboveMaximum)
                .WithData("pct", CommutationPct)
                .WithData("maximum", scheme.MaxCommutationPct);
        }

        var service = YearsBetween(joined, RetirementDate);
        PensionableServiceYears = scheme.MaxPensionableServiceYears > 0 ? Math.Min(service, scheme.MaxPensionableServiceYears) : service;

        AccrualRatePct = scheme.AccrualRatePct;
        CommutationFactor = scheme.CommutationFactor;

        var yearsEarly = Math.Max(0m, scheme.NormalRetirementAge - AgeAtRetirement);
        EarlyReductionPct = Math.Min(100m, Math.Round(yearsEarly * scheme.EarlyRetirementReductionPct, 4, MidpointRounding.AwayFromZero));

        FullAnnualPension = Round(salary * PensionableServiceYears * AccrualRatePct / 100m);
        ReducedAnnualPension = Round(FullAnnualPension * (100m - EarlyReductionPct) / 100m);
        CommutedAnnualPension = Round(ReducedAnnualPension * CommutationPct / 100m);
        LumpSum = Round(CommutedAnnualPension * CommutationFactor);
        AnnualPension = ReducedAnnualPension - CommutedAnnualPension;
        MonthlyPension = Round(AnnualPension / 12m);

        if (FinalPensionableSalary == 0m)
        {
            FinalPensionableSalary = salary;
        }
    }

    internal void MarkApproved(string pensionerNo, string paymentVoucherNo, DateTime when, string by)
    {
        Status = BenefitCalculationStatus.Approved;
        PensionerNo = pensionerNo;
        PaymentVoucherNo = paymentVoucherNo;
        ApprovedDate = when;
        ApprovedBy = by;
    }

    public void EnsureOpen()
    {
        if (Status != BenefitCalculationStatus.Open)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.DocumentNotOpen).WithData("documentNo", No ?? string.Empty);
        }
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static BusinessException MissingData(PensionMember member, string field) =>
        new BusinessException(ErpErrorCodes.Pensions.MemberDataMissing).WithData("memberNo", member.No).WithData("field", field);
}

/// <summary>Calculates defined benefit pensions and retires members onto them.</summary>
public class DefinedBenefitCalculator : DomainService
{
    public const string SourceType = "PensionBenefitCalculation";

    private readonly IRepository<PensionBenefitCalculation, Guid> _calculations;
    private readonly IRepository<PensionMember, Guid> _members;
    private readonly IRepository<Pensioner, Guid> _pensioners;
    private readonly PensionSchemeManager _schemeManager;
    private readonly PensionSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly PaymentVoucherFactory _voucherFactory;
    private readonly ICurrentUser _currentUser;

    public DefinedBenefitCalculator(
        IRepository<PensionBenefitCalculation, Guid> calculations,
        IRepository<PensionMember, Guid> members,
        IRepository<Pensioner, Guid> pensioners,
        PensionSchemeManager schemeManager,
        PensionSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        PaymentVoucherFactory voucherFactory,
        ICurrentUser currentUser
    )
    {
        _calculations = calculations;
        _members = members;
        _pensioners = pensioners;
        _schemeManager = schemeManager;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _voucherFactory = voucherFactory;
        _currentUser = currentUser;
    }

    public async Task CalculateAsync(PensionBenefitCalculation calculation)
    {
        var member = await GetMemberAsync(calculation.MemberNo);
        calculation.Calculate(member, await _schemeManager.GetAsync(member.SchemeCode));
        await _calculations.UpdateAsync(calculation, autoSave: true);
    }

    /// <summary>
    /// Works the pension out again as at today's member and scheme, then makes the member a
    /// pensioner from the month after retirement and raises the lump sum's payment voucher.
    /// </summary>
    public async Task ApproveAsync(PensionBenefitCalculation calculation)
    {
        var member = await GetMemberAsync(calculation.MemberNo);
        var scheme = await _schemeManager.GetAsync(member.SchemeCode);
        calculation.Calculate(member, scheme);

        if (calculation.MonthlyPension <= 0m && calculation.LumpSum <= 0m)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NothingToPost).WithData("documentNo", calculation.No);
        }

        var setup = await _setupManager.GetAsync();
        string voucherNo = null;
        if (calculation.LumpSum > 0m)
        {
            var account = setup.Require(setup.PensionsPaidAccountNo, "Pensions Paid Account No.");
            var narration = $"Commuted lump sum {calculation.No} {member.No}";
            var voucher = await _voucherFactory.CreateAsync(
                SourceType,
                calculation.No,
                calculation.RetirementDate,
                member.FullName,
                narration,
                [new PaymentVoucherRequestLine(GenJournalAccountType.GLAccount, account, narration, calculation.LumpSum)]
            );
            voucherNo = voucher.No;
        }

        string pensionerNo = null;
        if (calculation.MonthlyPension > 0m)
        {
            pensionerNo = (await _noSeriesManager.ResolveNoAsync(setup.PensionerNos, null, calculation.RetirementDate)).ToUpperInvariant();
            var firstMonth = new DateTime(calculation.RetirementDate.Year, calculation.RetirementDate.Month, 1).AddMonths(1);

            var pensioner = new Pensioner(GuidGenerator.Create(), pensionerNo, member.SchemeCode, member.FullName, firstMonth);
            pensioner.Set(member.SchemeCode, member.No, member.FullName, member.NationalId, member.TaxPinNo, member.DateOfBirth);
            pensioner.SetPension(calculation.MonthlyPension, firstMonth, null, PensionerStatus.Active);
            pensioner.SetContact(member.PhoneNo, member.Email, member.BankName, member.BankBranch, member.BankAccountNo);
            await _pensioners.InsertAsync(pensioner, autoSave: true);
        }

        member.Exit(MemberStatus.Inactive, calculation.RetirementDate);
        await _members.UpdateAsync(member, autoSave: true);

        calculation.MarkApproved(pensionerNo, voucherNo, Clock.Now, _currentUser.UserName);
        await _calculations.UpdateAsync(calculation, autoSave: true);
    }

    private async Task<PensionMember> GetMemberAsync(string memberNo)
    {
        return await _members.FirstOrDefaultAsync(m => m.No == memberNo)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Member").WithData("code", memberNo ?? string.Empty);
    }
}
