using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Pensions;

/// <summary>
/// Pension Setup: one row per company with the number series and G/L accounts the pension
/// processes use, and the dimension whose values are the schemes.
/// <para>
/// The system this module is modelled on keeps each scheme in a company of its own. Here every
/// scheme of an administrator lives in one company and is a value of one dimension, so the
/// schemes share a chart of accounts, sponsors can be reported across schemes, and any G/L report
/// can still be cut down to one scheme by filtering on that dimension.
/// </para>
/// </summary>
public class PensionSetup : CompanyEntity
{
    /// <summary>The dimension whose values are the schemes, e.g. "SCHEME".</summary>
    public string SchemeDimensionCode { get; private set; }

    public string MemberNos { get; private set; }
    public string SponsorNos { get; private set; }
    public string ContributionNos { get; private set; }
    public string InterestBatchNos { get; private set; }
    public string ExitNos { get; private set; }

    /// <summary>The liability to members: credited by contributions and interest, debited by exits.</summary>
    public string MemberFundsAccountNo { get; private set; }

    /// <summary>Contributions due from a sponsor that is not set up as a customer.</summary>
    public string ContributionAccrualAccountNo { get; private set; }

    /// <summary>Investment income distributed to members when interest is allocated.</summary>
    public string InterestAccountNo { get; private set; }

    /// <summary>Net benefits owed to members who have exited, until they are paid.</summary>
    public string BenefitsPayableAccountNo { get; private set; }

    /// <summary>Tax withheld from lump sums, owed to the revenue authority.</summary>
    public string TaxAccountNo { get; private set; }

    /// <summary>A second schedule for the same sponsor and period is refused unless this is on.</summary>
    public bool AllowContributionDuplication { get; private set; }

    public int NoOfDaysInAYear { get; private set; } = 365;

    protected PensionSetup() { }

    public PensionSetup(Guid id)
        : base(id) { }

    public void SetSchemeDimension(string schemeDimensionCode)
    {
        SchemeDimensionCode = CodeTableEntity.NormalizeCode(
            Check.Length(schemeDimensionCode, nameof(schemeDimensionCode), ErpDomainConsts.MaxDimensionCodeLength)
        );
    }

    public void SetNumbering(string memberNos, string sponsorNos, string contributionNos, string interestBatchNos, string exitNos)
    {
        MemberNos = Series(memberNos, nameof(memberNos));
        SponsorNos = Series(sponsorNos, nameof(sponsorNos));
        ContributionNos = Series(contributionNos, nameof(contributionNos));
        InterestBatchNos = Series(interestBatchNos, nameof(interestBatchNos));
        ExitNos = Series(exitNos, nameof(exitNos));
    }

    public void SetAccounts(
        string memberFundsAccountNo,
        string contributionAccrualAccountNo,
        string interestAccountNo,
        string benefitsPayableAccountNo,
        string taxAccountNo
    )
    {
        MemberFundsAccountNo = Account(memberFundsAccountNo, nameof(memberFundsAccountNo));
        ContributionAccrualAccountNo = Account(contributionAccrualAccountNo, nameof(contributionAccrualAccountNo));
        InterestAccountNo = Account(interestAccountNo, nameof(interestAccountNo));
        BenefitsPayableAccountNo = Account(benefitsPayableAccountNo, nameof(benefitsPayableAccountNo));
        TaxAccountNo = Account(taxAccountNo, nameof(taxAccountNo));
    }

    public string PensionerNos { get; private set; }
    public string PayrollNos { get; private set; }

    /// <summary>Pensions paid to pensioners: debited with the gross of every pension payroll.</summary>
    public string PensionsPaidAccountNo { get; private set; }

    public void SetPensionPayroll(string pensionerNos, string payrollNos, string pensionsPaidAccountNo)
    {
        PensionerNos = Series(pensionerNos, nameof(pensionerNos));
        PayrollNos = Series(payrollNos, nameof(payrollNos));
        PensionsPaidAccountNo = Account(pensionsPaidAccountNo, nameof(pensionsPaidAccountNo));
    }

    public string BenefitCalculationNos { get; private set; }

    public void SetBenefitCalculationNumbering(string benefitCalculationNos)
    {
        BenefitCalculationNos = Series(benefitCalculationNos, nameof(benefitCalculationNos));
    }

    /// <summary>
    /// What a transfer from another scheme is due from until the other scheme pays it: debited when
    /// a transfer-in schedule is posted, in place of the sponsor.
    /// </summary>
    public string TransfersInAccountNo { get; private set; }

    /// <summary>How contributions above the monthly tax relief limit are shared between employee and employer.</summary>
    public ExcessContributionAllocation ExcessContributionAllocation { get; private set; }

    /// <summary>How often a pensioner must prove they are alive; zero never asks.</summary>
    public int LifeCertificateFrequencyMonths { get; private set; } = 12;

    public string IncrementNos { get; private set; }

    public void SetMemberAdministration(
        string transfersInAccountNo,
        ExcessContributionAllocation excessContributionAllocation,
        int lifeCertificateFrequencyMonths,
        string incrementNos
    )
    {
        if (lifeCertificateFrequencyMonths is < 0 or > 120)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Life Certificate Frequency (Months)");
        }

        TransfersInAccountNo = Account(transfersInAccountNo, nameof(transfersInAccountNo));
        ExcessContributionAllocation = excessContributionAllocation;
        LifeCertificateFrequencyMonths = lifeCertificateFrequencyMonths;
        IncrementNos = Series(incrementNos, nameof(incrementNos));
    }

    /// <summary>The pay mode a new pensioner is given when none is chosen.</summary>
    public string DefaultPayModeCode { get; private set; }

    /// <summary>
    /// A monthly pension below this is too small to be worth paying: a defined benefit calculation
    /// commutes all of it to a lump sum instead. Zero commutes none.
    /// </summary>
    public decimal TrivialPensionLimit { get; private set; }

    public void SetPensionerDefaults(string defaultPayModeCode, decimal trivialPensionLimit)
    {
        if (trivialPensionLimit < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Trivial Pension Limit");
        }

        DefaultPayModeCode = CodeTableEntity.NormalizeCode(Check.Length(defaultPayModeCode, nameof(defaultPayModeCode), ErpDomainConsts.MaxCodeLength));
        TrivialPensionLimit = trivialPensionLimit;
    }

    public void SetOptions(bool allowContributionDuplication, int noOfDaysInAYear)
    {
        AllowContributionDuplication = allowContributionDuplication;
        NoOfDaysInAYear = noOfDaysInAYear is 360 or 365 or 366 ? noOfDaysInAYear : 365;
    }

    /// <summary>The account a process needs, or the error that names the setup field left blank.</summary>
    public string Require(string accountNo, string field)
    {
        return accountNo.IsNullOrWhiteSpace()
            ? throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing).WithData("field", field).WithData("setup", "Pension Setup")
            : accountNo;
    }

    private static string Series(string value, string name) =>
        value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, ErpDomainConsts.MaxNoSeriesCodeLength);

    private static string Account(string value, string name) =>
        value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, ErpDomainConsts.MaxNoLength);
}

public class PensionSetupManager : DomainService
{
    private readonly IRepository<PensionSetup, Guid> _repository;

    public PensionSetupManager(IRepository<PensionSetup, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The current company's setup, created blank on first use.</summary>
    public async Task<PensionSetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new PensionSetup(GuidGenerator.Create()), autoSave: true);
    }
}

/// <summary>
/// A pension scheme. Its code is a value of the scheme dimension named in the Pension Setup, and
/// that value is carried by every member, every schedule and every G/L entry the scheme posts.
/// What the original system keeps on Company Information per scheme is kept here.
/// </summary>
public class PensionScheme : CodeTableEntity
{
    public PensionSchemeType SchemeType { get; private set; }
    public PensionPlanType PlanType { get; private set; } = PensionPlanType.DefinedContribution;
    public PensionSchemeMode SchemeMode { get; private set; }
    public PensionSchemeStatus Status { get; private set; }
    public InterestCalculationMode InterestCalculationMode { get; private set; }

    /// <summary>The scheme's registration number with the retirement benefits regulator.</summary>
    public string RegulatorReferenceNo { get; private set; }

    /// <summary>The scheme's tax identification number.</summary>
    public string TaxPinNo { get; private set; }

    public int NormalRetirementAge { get; private set; } = 60;

    public int MinimumRetirementAge { get; private set; } = 50;

    protected PensionScheme() { }

    public PensionScheme(Guid id, string code, string name)
        : base(id, code, name) { }

    public void Set(
        PensionSchemeType schemeType,
        PensionPlanType planType,
        PensionSchemeMode schemeMode,
        PensionSchemeStatus status,
        InterestCalculationMode interestCalculationMode,
        string regulatorReferenceNo,
        string taxPinNo,
        int normalRetirementAge,
        int minimumRetirementAge
    )
    {
        if (normalRetirementAge is < 18 or > 100 || minimumRetirementAge is < 18 or > 100 || minimumRetirementAge > normalRetirementAge)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.InvalidRetirementAge);
        }

        SchemeType = schemeType;
        PlanType = planType;
        SchemeMode = schemeMode;
        Status = status;
        InterestCalculationMode = interestCalculationMode;
        RegulatorReferenceNo = Check.Length(regulatorReferenceNo, nameof(regulatorReferenceNo), ErpDomainConsts.MaxExternalDocumentNoLength);
        TaxPinNo = Check.Length(taxPinNo, nameof(taxPinNo), ErpDomainConsts.MaxVatRegistrationNoLength);
        NormalRetirementAge = normalRetirementAge;
        MinimumRetirementAge = minimumRetirementAge;
    }

    /// <summary>The share of final pensionable salary a year of service earns as annual pension, e.g. 2 for 1/50.</summary>
    public decimal AccrualRatePct { get; private set; }

    /// <summary>Service beyond this many years earns nothing more; 0 is no limit.</summary>
    public int MaxPensionableServiceYears { get; private set; }

    /// <summary>The largest share of the annual pension a retiree may give up for a lump sum.</summary>
    public decimal MaxCommutationPct { get; private set; }

    /// <summary>What one unit of annual pension given up is worth as a lump sum.</summary>
    public decimal CommutationFactor { get; private set; }

    /// <summary>How much the pension is cut for each year a member retires before the normal retirement age.</summary>
    public decimal EarlyRetirementReductionPct { get; private set; }

    /// <summary>The formula a defined benefit scheme pays pensions by.</summary>
    public void SetDefinedBenefit(
        decimal accrualRatePct,
        int maxPensionableServiceYears,
        decimal maxCommutationPct,
        decimal commutationFactor,
        decimal earlyRetirementReductionPct
    )
    {
        if (accrualRatePct is < 0 or > 100 || maxCommutationPct is < 0 or > 100 || earlyRetirementReductionPct is < 0 or > 100)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent)
                .WithData("value", accrualRatePct is < 0 or > 100 ? accrualRatePct : maxCommutationPct is < 0 or > 100 ? maxCommutationPct : earlyRetirementReductionPct);
        }

        if (maxPensionableServiceYears < 0 || commutationFactor < 0)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", maxPensionableServiceYears < 0 ? "Max. Pensionable Service Years" : "Commutation Factor");
        }

        AccrualRatePct = accrualRatePct;
        MaxPensionableServiceYears = maxPensionableServiceYears;
        MaxCommutationPct = maxCommutationPct;
        CommutationFactor = commutationFactor;
        EarlyRetirementReductionPct = earlyRetirementReductionPct;
    }

    /// <summary>The salary a pension is based on when the calculation has none keyed in.</summary>
    public PensionableSalaryBasis PensionableSalaryBasis { get; private set; }

    /// <summary>How many years before retirement the salary history is read over, for an average or highest salary.</summary>
    public int SalaryAveragingYears { get; private set; } = 3;

    public void SetPensionableSalary(PensionableSalaryBasis basis, int salaryAveragingYears)
    {
        if (salaryAveragingYears is < 0 or > 50)
        {
            throw new BusinessException(ErpErrorCodes.Pensions.NegativeAmount).WithData("field", "Salary Averaging Years");
        }

        PensionableSalaryBasis = basis;
        SalaryAveragingYears = salaryAveragingYears == 0 ? 3 : salaryAveragingYears;
    }

    /// <summary>Whether the scheme promises pensions by formula, alone or alongside a contribution pot.</summary>
    public bool PaysDefinedBenefit => PlanType is PensionPlanType.DefinedBenefit or PensionPlanType.Hybrid;
}

/// <summary>Keeps schemes and the scheme dimension in step, and turns a scheme into the dimension set its postings carry.</summary>
public class PensionSchemeManager : DomainService
{
    private readonly PensionSetupManager _setupManager;
    private readonly IRepository<PensionScheme, Guid> _schemes;
    private readonly IRepository<Dimension, Guid> _dimensions;
    private readonly IRepository<DimensionValue, Guid> _dimensionValues;
    private readonly IRepository<DimensionSetEntry, Guid> _dimensionSetEntries;
    private readonly DimensionManagement _dimensionManagement;

    public PensionSchemeManager(
        PensionSetupManager setupManager,
        IRepository<PensionScheme, Guid> schemes,
        IRepository<Dimension, Guid> dimensions,
        IRepository<DimensionValue, Guid> dimensionValues,
        IRepository<DimensionSetEntry, Guid> dimensionSetEntries,
        DimensionManagement dimensionManagement
    )
    {
        _setupManager = setupManager;
        _schemes = schemes;
        _dimensions = dimensions;
        _dimensionValues = dimensionValues;
        _dimensionSetEntries = dimensionSetEntries;
        _dimensionManagement = dimensionManagement;
    }

    public async Task<PensionScheme> GetAsync(string schemeCode)
    {
        var code = CodeTableEntity.NormalizeCode(schemeCode);
        return await _schemes.FirstOrDefaultAsync(s => s.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Pension Scheme").WithData("code", code ?? string.Empty);
    }

    /// <summary>A scheme that still takes members and contributions.</summary>
    public async Task<PensionScheme> GetOpenAsync(string schemeCode)
    {
        var scheme = await GetAsync(schemeCode);
        return scheme.Status == PensionSchemeStatus.Closed
            ? throw new BusinessException(ErpErrorCodes.Pensions.SchemeClosed).WithData("scheme", scheme.Code)
            : scheme;
    }

    /// <summary>The dimension the schemes are values of; the setup must name it.</summary>
    public async Task<Dimension> GetSchemeDimensionAsync()
    {
        var setup = await _setupManager.GetAsync();
        var code = setup.Require(setup.SchemeDimensionCode, "Scheme Dimension Code");

        return await _dimensions.FirstOrDefaultAsync(d => d.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Dimension").WithData("code", code);
    }

    /// <summary>Makes sure the scheme exists as a value of the scheme dimension, under the scheme's own name.</summary>
    public async Task EnsureDimensionValueAsync(PensionScheme scheme)
    {
        var dimension = await GetSchemeDimensionAsync();
        var value = await _dimensionValues.FirstOrDefaultAsync(v => v.DimensionId == dimension.Id && v.Code == scheme.Code);
        var name = scheme.Description.IsNullOrWhiteSpace() ? scheme.Code : scheme.Description;

        if (value == null)
        {
            await _dimensionValues.InsertAsync(
                new DimensionValue(GuidGenerator.Create(), dimension.Id, dimension.Code, scheme.Code, Truncate(name)),
                autoSave: true
            );
        }
        else if (value.Name != Truncate(name))
        {
            value.SetName(Truncate(name));
            await _dimensionValues.UpdateAsync(value, autoSave: true);
        }
    }

    /// <summary>
    /// The dimension set a scheme's entries are posted with: the scheme dimension and the scheme's
    /// code. It is stored on first use so that the set can be read back from any entry.
    /// </summary>
    public async Task<Guid> GetDimensionSetIdAsync(string schemeCode)
    {
        var dimension = await GetSchemeDimensionAsync();
        var code = CodeTableEntity.NormalizeCode(schemeCode);
        var setId = _dimensionManagement.GetDimensionSetId(new Dictionary<string, string> { [dimension.Code] = code });

        if (!await _dimensionSetEntries.AnyAsync(e => e.DimensionSetId == setId))
        {
            await _dimensionSetEntries.InsertAsync(new DimensionSetEntry(GuidGenerator.Create(), setId, dimension.Code, code), autoSave: true);
        }

        return setId;
    }

    private static string Truncate(string name) =>
        name.Length > ErpDomainConsts.MaxNameLength ? name[..ErpDomainConsts.MaxNameLength] : name;
}
