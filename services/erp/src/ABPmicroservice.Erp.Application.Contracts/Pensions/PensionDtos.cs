using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Pensions;

// ---------------------------------------------------------------------------- Setup

public class PensionSetupDto
{
    [StringLength(ErpDomainConsts.MaxDimensionCodeLength)]
    public string SchemeDimensionCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string MemberNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string SponsorNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ContributionNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string InterestBatchNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string ExitNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string MemberFundsAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ContributionAccrualAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string InterestAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string BenefitsPayableAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string TaxAccountNo { get; set; }

    public bool AllowContributionDuplication { get; set; }

    public int NoOfDaysInAYear { get; set; } = 365;

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string PensionerNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string PayrollNos { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PensionsPaidAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string BenefitCalculationNos { get; set; }
}

public interface IPensionSetupAppService : IApplicationService
{
    Task<PensionSetupDto> GetAsync();

    Task<PensionSetupDto> UpdateAsync(PensionSetupDto input);
}

// ---------------------------------------------------------------------------- Schemes

public class PensionSchemeDto : CodeTableDto
{
    public PensionSchemeType SchemeType { get; set; }
    public PensionPlanType PlanType { get; set; }
    public PensionSchemeMode SchemeMode { get; set; }
    public PensionSchemeStatus Status { get; set; }
    public InterestCalculationMode InterestCalculationMode { get; set; }
    public string RegulatorReferenceNo { get; set; }
    public string TaxPinNo { get; set; }
    public int NormalRetirementAge { get; set; }
    public int MinimumRetirementAge { get; set; }
    public decimal AccrualRatePct { get; set; }
    public int MaxPensionableServiceYears { get; set; }
    public decimal MaxCommutationPct { get; set; }
    public decimal CommutationFactor { get; set; }
    public decimal EarlyRetirementReductionPct { get; set; }
}

public class CreateUpdatePensionSchemeDto : CreateUpdateCodeTableDto
{
    public PensionSchemeType SchemeType { get; set; }
    public PensionPlanType PlanType { get; set; } = PensionPlanType.DefinedContribution;
    public PensionSchemeMode SchemeMode { get; set; }
    public PensionSchemeStatus Status { get; set; }
    public InterestCalculationMode InterestCalculationMode { get; set; }

    [StringLength(ErpDomainConsts.MaxExternalDocumentNoLength)]
    public string RegulatorReferenceNo { get; set; }

    [StringLength(ErpDomainConsts.MaxVatRegistrationNoLength)]
    public string TaxPinNo { get; set; }

    [Range(18, 100)]
    public int NormalRetirementAge { get; set; } = 60;

    [Range(18, 100)]
    public int MinimumRetirementAge { get; set; } = 50;

    /// <summary>Defined benefit only: the share of final salary a year of service earns as annual pension.</summary>
    [Range(0, 100)]
    public decimal AccrualRatePct { get; set; }

    [Range(0, 100)]
    public int MaxPensionableServiceYears { get; set; }

    [Range(0, 100)]
    public decimal MaxCommutationPct { get; set; }

    [Range(0, 1000)]
    public decimal CommutationFactor { get; set; }

    [Range(0, 100)]
    public decimal EarlyRetirementReductionPct { get; set; }
}

public interface IPensionSchemeAppService
    : ICrudAppService<PensionSchemeDto, Guid, GetCodeTableListInput, CreateUpdatePensionSchemeDto, CreateUpdatePensionSchemeDto> { }

// ---------------------------------------------------------------------------- Sponsors

public class PensionSponsorDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Name { get; set; }
    public string SchemeCode { get; set; }
    public string CustomerNo { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public string Contact { get; set; }
    public string TaxPinNo { get; set; }
    public decimal EmployeeRatePct { get; set; }
    public decimal EmployerRatePct { get; set; }
    public DateTime? LastScheduleDate { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdatePensionSponsorDto
{
    /// <summary>Blank takes the next number of the Pension Setup's Sponsor Nos.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDimensionValueCodeLength)]
    public string SchemeCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CustomerNo { get; set; }

    [StringLength(ErpDomainConsts.MaxAddressLength)]
    public string Address { get; set; }

    [StringLength(ErpDomainConsts.MaxCityLength)]
    public string City { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailLength)]
    public string Email { get; set; }

    [StringLength(ErpDomainConsts.MaxContactLength)]
    public string Contact { get; set; }

    [StringLength(ErpDomainConsts.MaxVatRegistrationNoLength)]
    public string TaxPinNo { get; set; }

    [Range(0, 100)]
    public decimal EmployeeRatePct { get; set; }

    [Range(0, 100)]
    public decimal EmployerRatePct { get; set; }

    public bool Blocked { get; set; }
}

public class GetPensionSponsorListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string SchemeCode { get; set; }
}

public interface IPensionSponsorAppService
    : ICrudAppService<PensionSponsorDto, Guid, GetPensionSponsorListInput, CreateUpdatePensionSponsorDto, CreateUpdatePensionSponsorDto> { }

// ---------------------------------------------------------------------------- Members

public class PensionMemberDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string SchemeCode { get; set; }
    public string SponsorNo { get; set; }
    public string FirstName { get; set; }
    public string OtherName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string NationalId { get; set; }
    public string TaxPinNo { get; set; }
    public MemberGender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public MemberMaritalStatus MaritalStatus { get; set; }
    public string PayrollNo { get; set; }
    public string Designation { get; set; }
    public DateTime? DateOfEmployment { get; set; }
    public DateTime? JoinSchemeDate { get; set; }
    public decimal CurrentSalary { get; set; }
    public DateTime? ExpectedRetirementDate { get; set; }
    public MemberStatus Status { get; set; }
    public MemberContributionStatus ContributionStatus { get; set; }
    public DateTime? ExitDate { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public string BankName { get; set; }
    public string BankBranch { get; set; }
    public string BankAccountNo { get; set; }
}

public class CreateUpdatePensionMemberDto
{
    /// <summary>Blank takes the next number of the Pension Setup's Member Nos.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SponsorNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength / 2)]
    public string FirstName { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength / 2)]
    public string OtherName { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength / 2)]
    public string LastName { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength * 2)]
    public string NationalId { get; set; }

    [StringLength(ErpDomainConsts.MaxVatRegistrationNoLength)]
    public string TaxPinNo { get; set; }

    public MemberGender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public MemberMaritalStatus MaritalStatus { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PayrollNo { get; set; }

    [StringLength(ErpDomainConsts.MaxJobTitleLength)]
    public string Designation { get; set; }

    public DateTime? DateOfEmployment { get; set; }
    public DateTime? JoinSchemeDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CurrentSalary { get; set; }

    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public MemberContributionStatus ContributionStatus { get; set; }

    [StringLength(ErpDomainConsts.MaxAddressLength)]
    public string Address { get; set; }

    [StringLength(ErpDomainConsts.MaxCityLength)]
    public string City { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailLength)]
    public string Email { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string BankName { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string BankBranch { get; set; }

    [StringLength(ErpDomainConsts.MaxBankAccountNoLength)]
    public string BankAccountNo { get; set; }
}

public class GetPensionMemberListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string SchemeCode { get; set; }
    public string SponsorNo { get; set; }
    public MemberStatus? Status { get; set; }
}

/// <summary>One money type of a member's fund.</summary>
public class MemberBalanceLineDto
{
    public PensionContributionType ContributionType { get; set; }
    public PensionExemptionType ExemptionType { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>A member's fund as at a date: the totals, and the same money by type.</summary>
public class MemberBalanceDto
{
    public string MemberNo { get; set; }
    public DateTime AsOfDate { get; set; }
    public decimal Total { get; set; }
    public decimal Employee { get; set; }
    public decimal Employer { get; set; }
    public decimal Registered { get; set; }
    public decimal Unregistered { get; set; }
    public List<MemberBalanceLineDto> Lines { get; set; } = new();
}

public interface IPensionMemberAppService
    : ICrudAppService<PensionMemberDto, Guid, GetPensionMemberListInput, CreateUpdatePensionMemberDto, CreateUpdatePensionMemberDto>
{
    /// <summary>Routed as GET /api/erp/pension-member/{id}/balance.</summary>
    Task<MemberBalanceDto> GetBalanceAsync(Guid id, DateTime? asOfDate = null);
}

public class MemberLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public string SchemeCode { get; set; }
    public string MemberNo { get; set; }
    public string SponsorNo { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime? ContributionPeriod { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public PensionTransactionType TransactionType { get; set; }
    public PensionContributionType ContributionType { get; set; }
    public PensionContributionMode ContributionMode { get; set; }
    public PensionExemptionType ExemptionType { get; set; }
    public decimal Amount { get; set; }
    public decimal Salary { get; set; }
    public string UserName { get; set; }
}

public class GetMemberLedgerEntryListInput : ErpPagedListInput
{
    /// <summary>Matches the member number, the document number or the description.</summary>
    public string Filter { get; set; }
    public string MemberNo { get; set; }
    public string SchemeCode { get; set; }
    public string DocumentNo { get; set; }
}

public interface IMemberLedgerEntryAppService : IReadOnlyAppService<MemberLedgerEntryDto, Guid, GetMemberLedgerEntryListInput> { }

// ---------------------------------------------------------------------------- Contributions

public class PensionContributionHeaderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string SchemeCode { get; set; }
    public string SponsorNo { get; set; }
    public string SponsorName { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime ContributionPeriod { get; set; }
    public string Description { get; set; }
    public PensionContributionMode ContributionMode { get; set; }
    public PensionDocumentStatus Status { get; set; }
    public DateTime? PostedDate { get; set; }
    public string PostedBy { get; set; }
    public decimal TotalAmount { get; set; }
    public int NoOfMembers { get; set; }
}

public class CreateUpdatePensionContributionHeaderDto
{
    /// <summary>Blank takes the next number of the Pension Setup's Contribution Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SponsorNo { get; set; }

    public DateTime PostingDate { get; set; }

    /// <summary>Any day of the month the contributions are for.</summary>
    public DateTime ContributionPeriod { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public PensionContributionMode ContributionMode { get; set; } = PensionContributionMode.Normal;
}

public class GetPensionContributionListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string SponsorNo { get; set; }
    public PensionDocumentStatus? Status { get; set; }
}

public class PensionContributionLineDto : FullAuditedEntityDto<Guid>
{
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string MemberNo { get; set; }
    public string MemberName { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal EmployeeTaxExempt { get; set; }
    public decimal EmployeeNonTaxExempt { get; set; }
    public decimal EmployeeAvcTaxExempt { get; set; }
    public decimal EmployeeAvcNonTaxExempt { get; set; }
    public decimal EmployerTaxExempt { get; set; }
    public decimal EmployerNonTaxExempt { get; set; }
    public decimal EmployerAvcTaxExempt { get; set; }
    public decimal EmployerAvcNonTaxExempt { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CreateUpdatePensionContributionLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    /// <summary>Zero takes the next free number.</summary>
    public int LineNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string MemberNo { get; set; }

    [Range(0, double.MaxValue)]
    public decimal BasicSalary { get; set; }

    [Range(0, double.MaxValue)]
    public decimal EmployeeTaxExempt { get; set; }

    [Range(0, double.MaxValue)]
    public decimal EmployeeNonTaxExempt { get; set; }

    [Range(0, double.MaxValue)]
    public decimal EmployeeAvcTaxExempt { get; set; }

    [Range(0, double.MaxValue)]
    public decimal EmployeeAvcNonTaxExempt { get; set; }

    [Range(0, double.MaxValue)]
    public decimal EmployerTaxExempt { get; set; }

    [Range(0, double.MaxValue)]
    public decimal EmployerNonTaxExempt { get; set; }

    [Range(0, double.MaxValue)]
    public decimal EmployerAvcTaxExempt { get; set; }

    [Range(0, double.MaxValue)]
    public decimal EmployerAvcNonTaxExempt { get; set; }
}

public class GetPensionContributionLineListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string DocumentNo { get; set; }
}

public interface IPensionContributionAppService
    : ICrudAppService<PensionContributionHeaderDto, Guid, GetPensionContributionListInput, CreateUpdatePensionContributionHeaderDto, CreateUpdatePensionContributionHeaderDto>
{
    /// <summary>Adds a line for every contributing member of the sponsor. Routed as POST /api/erp/pension-contribution/{id}/suggest-lines.</summary>
    Task<PensionContributionHeaderDto> SuggestLinesAsync(Guid id);

    Task<PensionContributionHeaderDto> ReleaseAsync(Guid id);

    Task<PensionContributionHeaderDto> ReopenAsync(Guid id);

    /// <summary>Routed as POST /api/erp/pension-contribution/{id}/run-posting.</summary>
    Task<PensionContributionHeaderDto> RunPostingAsync(Guid id);
}

public interface IPensionContributionLineAppService
    : ICrudAppService<PensionContributionLineDto, Guid, GetPensionContributionLineListInput, CreateUpdatePensionContributionLineDto, CreateUpdatePensionContributionLineDto> { }

// ---------------------------------------------------------------------------- Interest

public class PensionInterestRateDto : FullAuditedEntityDto<Guid>
{
    public string SchemeCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? DateDeclared { get; set; }
    public decimal RegisteredRatePct { get; set; }
    public decimal UnregisteredRatePct { get; set; }
    public decimal TaxRatePct { get; set; }
    public bool Posted { get; set; }
    public string PostedDocumentNo { get; set; }
    public DateTime? PostedDate { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalTax { get; set; }
}

public class CreateUpdatePensionInterestRateDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDimensionValueCodeLength)]
    public string SchemeCode { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? DateDeclared { get; set; }

    [Range(0, 100)]
    public decimal RegisteredRatePct { get; set; }

    [Range(0, 100)]
    public decimal UnregisteredRatePct { get; set; }

    [Range(0, 100)]
    public decimal TaxRatePct { get; set; }
}

public class GetPensionInterestRateListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string SchemeCode { get; set; }
}

public class InterestAllocationLineDto
{
    public string MemberNo { get; set; }
    public string MemberName { get; set; }
    public PensionContributionType ContributionType { get; set; }
    public PensionExemptionType ExemptionType { get; set; }
    public decimal Balance { get; set; }
    public decimal Interest { get; set; }
    public decimal Tax { get; set; }
}

public class InterestAllocationDto
{
    public decimal TotalInterest { get; set; }
    public decimal TotalTax { get; set; }
    public int NoOfMembers { get; set; }
    public List<InterestAllocationLineDto> Lines { get; set; } = new();
}

public class AllocateInterestInput
{
    /// <summary>Blank posts on the last day of the declared period.</summary>
    public DateTime? PostingDate { get; set; }
}

public interface IPensionInterestRateAppService
    : ICrudAppService<PensionInterestRateDto, Guid, GetPensionInterestRateListInput, CreateUpdatePensionInterestRateDto, CreateUpdatePensionInterestRateDto>
{
    /// <summary>What allocating would credit, without posting. Routed as GET /api/erp/pension-interest-rate/{id}/preview.</summary>
    Task<InterestAllocationDto> GetPreviewAsync(Guid id);

    /// <summary>Routed as POST /api/erp/pension-interest-rate/{id}/allocate.</summary>
    Task<PensionInterestRateDto> AllocateAsync(Guid id, AllocateInterestInput input);
}

// ---------------------------------------------------------------------------- Exits

public class ExitReasonDto : CodeTableDto
{
    public ExitPaymentOption PaymentOption { get; set; }
    public decimal EmployerPortionPct { get; set; }
    public string TaxTableCode { get; set; }
    public bool LumpsumTaxFree { get; set; }
    public MemberStatus StatusAfterExit { get; set; }
}

public class CreateUpdateExitReasonDto : CreateUpdateCodeTableDto
{
    public ExitPaymentOption PaymentOption { get; set; } = ExitPaymentOption.PayEmployeeAndEmployer;

    [Range(0, 100)]
    public decimal EmployerPortionPct { get; set; } = 100m;

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string TaxTableCode { get; set; }

    public bool LumpsumTaxFree { get; set; }

    public MemberStatus StatusAfterExit { get; set; } = MemberStatus.Inactive;
}

public interface IExitReasonAppService
    : ICrudAppService<ExitReasonDto, Guid, GetCodeTableListInput, CreateUpdateExitReasonDto, CreateUpdateExitReasonDto> { }

public class LumpsumTaxTableDto : CodeTableDto
{
    public decimal AnnualTaxFreeAmount { get; set; }
    public decimal MaxTaxFreeAmount { get; set; }
    public int MaxAgeTaxable { get; set; }
}

public class CreateUpdateLumpsumTaxTableDto : CreateUpdateCodeTableDto
{
    [Range(0, double.MaxValue)]
    public decimal AnnualTaxFreeAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MaxTaxFreeAmount { get; set; }

    [Range(0, 150)]
    public int MaxAgeTaxable { get; set; }
}

public interface ILumpsumTaxTableAppService
    : ICrudAppService<LumpsumTaxTableDto, Guid, GetCodeTableListInput, CreateUpdateLumpsumTaxTableDto, CreateUpdateLumpsumTaxTableDto> { }

public class LumpsumTaxBandDto : FullAuditedEntityDto<Guid>
{
    public string TaxTableCode { get; set; }
    public decimal LowerLimit { get; set; }
    public decimal UpperLimit { get; set; }
    public decimal RatePct { get; set; }
}

public class CreateUpdateLumpsumTaxBandDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string TaxTableCode { get; set; }

    [Range(0, double.MaxValue)]
    public decimal LowerLimit { get; set; }

    /// <summary>Zero means the band has no upper limit.</summary>
    [Range(0, double.MaxValue)]
    public decimal UpperLimit { get; set; }

    [Range(0, 100)]
    public decimal RatePct { get; set; }
}

public class GetLumpsumTaxBandListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string TaxTableCode { get; set; }
}

public interface ILumpsumTaxBandAppService
    : ICrudAppService<LumpsumTaxBandDto, Guid, GetLumpsumTaxBandListInput, CreateUpdateLumpsumTaxBandDto, CreateUpdateLumpsumTaxBandDto> { }

public class MemberExitDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string MemberNo { get; set; }
    public string MemberName { get; set; }
    public string SchemeCode { get; set; }
    public string SponsorNo { get; set; }
    public string ReasonCode { get; set; }
    public MemberWithdrawalType WithdrawalType { get; set; }
    public DateTime ExitDate { get; set; }
    public DateTime DateOfCalculation { get; set; }
    public MemberExitStatus Status { get; set; }
    public string Comment { get; set; }
    public decimal AgeAtExit { get; set; }
    public decimal ServiceYears { get; set; }
    public decimal EmployeeBalance { get; set; }
    public decimal EmployerBalance { get; set; }
    public decimal EmployeePayable { get; set; }
    public decimal EmployerPayable { get; set; }
    public decimal DeferredAmount { get; set; }
    public decimal RegisteredPayable { get; set; }
    public decimal UnregisteredPayable { get; set; }
    public decimal GrossLumpsum { get; set; }
    public decimal TaxFreeAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxOnLumpsum { get; set; }
    public decimal NetPayable { get; set; }
    public DateTime? PostedDate { get; set; }
    public string PostedBy { get; set; }
    public string PaymentVoucherNo { get; set; }
}

public class CreateUpdateMemberExitDto
{
    /// <summary>Blank takes the next number of the Pension Setup's Exit Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string MemberNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ReasonCode { get; set; }

    public MemberWithdrawalType WithdrawalType { get; set; }

    public DateTime ExitDate { get; set; }

    /// <summary>Blank calculates as at the exit date.</summary>
    public DateTime? DateOfCalculation { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Comment { get; set; }
}

public class GetMemberExitListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string MemberNo { get; set; }
    public MemberExitStatus? Status { get; set; }
}

public class PostMemberExitInput
{
    /// <summary>Blank posts on the exit date.</summary>
    public DateTime? PostingDate { get; set; }
}

public interface IMemberExitAppService
    : ICrudAppService<MemberExitDto, Guid, GetMemberExitListInput, CreateUpdateMemberExitDto, CreateUpdateMemberExitDto>
{
    /// <summary>Works the benefit out again. Routed as POST /api/erp/member-exit/{id}/calculate.</summary>
    Task<MemberExitDto> CalculateAsync(Guid id);

    Task<MemberExitDto> ApproveAsync(Guid id);

    Task<MemberExitDto> ReopenAsync(Guid id);

    /// <summary>Routed as POST /api/erp/member-exit/{id}/run-posting.</summary>
    Task<MemberExitDto> RunPostingAsync(Guid id, PostMemberExitInput input);

    /// <summary>Raises the payment voucher that pays the net benefit. Routed as POST /api/erp/member-exit/{id}/raise-payment-voucher.</summary>
    Task<MemberExitDto> RaisePaymentVoucherAsync(Guid id);
}

// ---------------------------------------------------------------------------- Pensioners and pension payroll

public class PensionerDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string SchemeCode { get; set; }
    public string MemberNo { get; set; }
    public string Name { get; set; }
    public string NationalId { get; set; }
    public string TaxPinNo { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public decimal MonthlyPension { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PensionerStatus Status { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public string BankName { get; set; }
    public string BankBranch { get; set; }
    public string BankAccountNo { get; set; }
    public DateTime? LastPaidPeriod { get; set; }
}

public class CreateUpdatePensionerDto
{
    /// <summary>Blank takes the next number of the Pension Setup's Pensioner Nos.</summary>
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    /// <summary>Blank takes the scheme of the member named.</summary>
    [StringLength(ErpDomainConsts.MaxDimensionValueCodeLength)]
    public string SchemeCode { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string MemberNo { get; set; }

    /// <summary>Blank takes the name of the member named.</summary>
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength * 2)]
    public string NationalId { get; set; }

    [StringLength(ErpDomainConsts.MaxVatRegistrationNoLength)]
    public string TaxPinNo { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MonthlyPension { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PensionerStatus Status { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailLength)]
    public string Email { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string BankName { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string BankBranch { get; set; }

    [StringLength(ErpDomainConsts.MaxBankAccountNoLength)]
    public string BankAccountNo { get; set; }
}

public class GetPensionerListInput : ErpPagedListInput
{
    /// <summary>Matches the pensioner number, the name, the member number or the national ID.</summary>
    public string Filter { get; set; }
    public string SchemeCode { get; set; }
    public PensionerStatus? Status { get; set; }
}

public interface IPensionerAppService
    : ICrudAppService<PensionerDto, Guid, GetPensionerListInput, CreateUpdatePensionerDto, CreateUpdatePensionerDto> { }

public class PensionPayrollHeaderDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string SchemeCode { get; set; }
    public DateTime PayPeriod { get; set; }
    public DateTime PostingDate { get; set; }
    public string Description { get; set; }
    public decimal TaxRatePct { get; set; }
    public decimal TaxFreeAmount { get; set; }
    public PensionDocumentStatus Status { get; set; }
    public DateTime? PostedDate { get; set; }
    public string PostedBy { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalNet { get; set; }
    public int NoOfPensioners { get; set; }
    public string PaymentVoucherNo { get; set; }
}

public class CreateUpdatePensionPayrollHeaderDto
{
    /// <summary>Blank takes the next number of the Pension Setup's Payroll Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDimensionValueCodeLength)]
    public string SchemeCode { get; set; }

    /// <summary>Any day of the month the pensions are for.</summary>
    public DateTime PayPeriod { get; set; }

    public DateTime PostingDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Range(0, 100)]
    public decimal TaxRatePct { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TaxFreeAmount { get; set; }
}

public class GetPensionPayrollListInput : ErpPagedListInput
{
    /// <summary>Matches the payroll number, the scheme or the description.</summary>
    public string Filter { get; set; }
    public string SchemeCode { get; set; }
    public PensionDocumentStatus? Status { get; set; }
}

public interface IPensionPayrollAppService
    : ICrudAppService<PensionPayrollHeaderDto, Guid, GetPensionPayrollListInput, CreateUpdatePensionPayrollHeaderDto, CreateUpdatePensionPayrollHeaderDto>
{
    /// <summary>Adds a line for every pensioner due for the month. Routed as POST /api/erp/pension-payroll/{id}/suggest-lines.</summary>
    Task<PensionPayrollHeaderDto> SuggestLinesAsync(Guid id);

    Task<PensionPayrollHeaderDto> ReleaseAsync(Guid id);

    Task<PensionPayrollHeaderDto> ReopenAsync(Guid id);

    /// <summary>Routed as POST /api/erp/pension-payroll/{id}/run-posting.</summary>
    Task<PensionPayrollHeaderDto> RunPostingAsync(Guid id);

    /// <summary>Raises the payment voucher that pays the net. Routed as POST /api/erp/pension-payroll/{id}/raise-payment-voucher.</summary>
    Task<PensionPayrollHeaderDto> RaisePaymentVoucherAsync(Guid id);
}

public class PensionPayrollLineDto : FullAuditedEntityDto<Guid>
{
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string PensionerNo { get; set; }
    public string PensionerName { get; set; }
    public decimal GrossPension { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetPension { get; set; }
}

public class CreateUpdatePensionPayrollLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    /// <summary>0 puts the line after the last one.</summary>
    public int LineNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PensionerNo { get; set; }

    /// <summary>Left out, the pensioner's monthly pension.</summary>
    [Range(0, double.MaxValue)]
    public decimal? GrossPension { get; set; }

    /// <summary>Left out, the payroll's tax on the gross pension.</summary>
    [Range(0, double.MaxValue)]
    public decimal? TaxAmount { get; set; }
}

public class GetPensionPayrollLineListInput : ErpPagedListInput
{
    /// <summary>Matches the payroll number, the pensioner number or the pensioner's name.</summary>
    public string Filter { get; set; }
    public string DocumentNo { get; set; }
    public string PensionerNo { get; set; }
}

public interface IPensionPayrollLineAppService
    : ICrudAppService<PensionPayrollLineDto, Guid, GetPensionPayrollLineListInput, CreateUpdatePensionPayrollLineDto, CreateUpdatePensionPayrollLineDto> { }

// ---------------------------------------------------------------------------- Defined benefit calculations

public class PensionBenefitCalculationDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string MemberNo { get; set; }
    public string MemberName { get; set; }
    public string SchemeCode { get; set; }
    public DateTime CalculationDate { get; set; }
    public DateTime RetirementDate { get; set; }
    public decimal FinalPensionableSalary { get; set; }
    public decimal CommutationPct { get; set; }
    public string Comment { get; set; }
    public decimal AgeAtRetirement { get; set; }
    public decimal PensionableServiceYears { get; set; }
    public decimal AccrualRatePct { get; set; }
    public decimal CommutationFactor { get; set; }
    public decimal EarlyReductionPct { get; set; }
    public decimal FullAnnualPension { get; set; }
    public decimal ReducedAnnualPension { get; set; }
    public decimal CommutedAnnualPension { get; set; }
    public decimal LumpSum { get; set; }
    public decimal AnnualPension { get; set; }
    public decimal MonthlyPension { get; set; }
    public BenefitCalculationStatus Status { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string ApprovedBy { get; set; }
    public string PensionerNo { get; set; }
    public string PaymentVoucherNo { get; set; }
}

public class CreateUpdatePensionBenefitCalculationDto
{
    /// <summary>Blank takes the next number of the Pension Setup's Benefit Calculation Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string MemberNo { get; set; }

    public DateTime CalculationDate { get; set; }

    /// <summary>Blank takes the member's expected retirement date.</summary>
    public DateTime? RetirementDate { get; set; }

    /// <summary>Annual; zero takes twelve times the member's current monthly salary.</summary>
    [Range(0, double.MaxValue)]
    public decimal FinalPensionableSalary { get; set; }

    [Range(0, 100)]
    public decimal CommutationPct { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Comment { get; set; }
}

public class GetPensionBenefitCalculationListInput : ErpPagedListInput
{
    /// <summary>Matches the calculation number, the member number or the member's name.</summary>
    public string Filter { get; set; }
    public string SchemeCode { get; set; }
    public BenefitCalculationStatus? Status { get; set; }
}

public interface IPensionBenefitCalculationAppService
    : ICrudAppService<PensionBenefitCalculationDto, Guid, GetPensionBenefitCalculationListInput, CreateUpdatePensionBenefitCalculationDto, CreateUpdatePensionBenefitCalculationDto>
{
    /// <summary>Works the pension out again. Routed as POST /api/erp/pension-benefit-calculation/{id}/calculate.</summary>
    Task<PensionBenefitCalculationDto> CalculateAsync(Guid id);

    /// <summary>Makes the member a pensioner and raises the lump sum's voucher. Routed as POST /api/erp/pension-benefit-calculation/{id}/approve.</summary>
    Task<PensionBenefitCalculationDto> ApproveAsync(Guid id);
}
