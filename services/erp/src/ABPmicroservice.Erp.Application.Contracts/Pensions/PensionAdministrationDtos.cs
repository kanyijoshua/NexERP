using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Pensions;

// ---------------------------------------------------------------------------- Beneficiaries

public class PensionBeneficiaryDto : FullAuditedEntityDto<Guid>
{
    public string MemberNo { get; set; }
    public int LineNo { get; set; }
    public string Name { get; set; }
    public BeneficiaryRelationship Relationship { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string NationalId { get; set; }
    public decimal BenefitPct { get; set; }
    public BeneficiaryStatus Status { get; set; }
    public string GuardianName { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public string BankName { get; set; }
    public string BankAccountNo { get; set; }
}

public class CreateUpdatePensionBeneficiaryDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string MemberNo { get; set; }

    /// <summary>0 puts the beneficiary after the last one.</summary>
    public int LineNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    public BeneficiaryRelationship Relationship { get; set; }
    public DateTime? DateOfBirth { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength * 2)]
    public string NationalId { get; set; }

    [Range(0, 100)]
    public decimal BenefitPct { get; set; }

    public BeneficiaryStatus Status { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string GuardianName { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailLength)]
    public string Email { get; set; }

    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string BankName { get; set; }

    [StringLength(ErpDomainConsts.MaxBankAccountNoLength)]
    public string BankAccountNo { get; set; }
}

public class GetPensionBeneficiaryListInput : ErpPagedListInput
{
    /// <summary>Matches the member number or the beneficiary's name.</summary>
    public string Filter { get; set; }
    public string MemberNo { get; set; }
}

public interface IPensionBeneficiaryAppService
    : ICrudAppService<PensionBeneficiaryDto, Guid, GetPensionBeneficiaryListInput, CreateUpdatePensionBeneficiaryDto, CreateUpdatePensionBeneficiaryDto> { }

// ---------------------------------------------------------------------------- Sponsor contribution rates and vesting

public class PensionContributionRateDto : FullAuditedEntityDto<Guid>
{
    public string SponsorNo { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal EmployeeRatePct { get; set; }
    public decimal EmployerRatePct { get; set; }
}

public class CreateUpdatePensionContributionRateDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SponsorNo { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Range(0, 100)]
    public decimal EmployeeRatePct { get; set; }

    [Range(0, 100)]
    public decimal EmployerRatePct { get; set; }
}

public class GetPensionContributionRateListInput : ErpPagedListInput
{
    /// <summary>Matches the sponsor number.</summary>
    public string Filter { get; set; }
    public string SponsorNo { get; set; }
}

public interface IPensionContributionRateAppService
    : ICrudAppService<PensionContributionRateDto, Guid, GetPensionContributionRateListInput, CreateUpdatePensionContributionRateDto, CreateUpdatePensionContributionRateDto> { }

public class PensionVestingScaleDto : FullAuditedEntityDto<Guid>
{
    public string SponsorNo { get; set; }
    public decimal FromServiceYears { get; set; }
    public decimal EmployerVestedPct { get; set; }
}

public class CreateUpdatePensionVestingScaleDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SponsorNo { get; set; }

    [Range(0, 100)]
    public decimal FromServiceYears { get; set; }

    [Range(0, 100)]
    public decimal EmployerVestedPct { get; set; }
}

public class GetPensionVestingScaleListInput : ErpPagedListInput
{
    /// <summary>Matches the sponsor number.</summary>
    public string Filter { get; set; }
    public string SponsorNo { get; set; }
}

public interface IPensionVestingScaleAppService
    : ICrudAppService<PensionVestingScaleDto, Guid, GetPensionVestingScaleListInput, CreateUpdatePensionVestingScaleDto, CreateUpdatePensionVestingScaleDto> { }

// ---------------------------------------------------------------------------- Tax relief limits

public class PensionTaxReliefLimitDto : FullAuditedEntityDto<Guid>
{
    public DateTime EffectiveDate { get; set; }
    public decimal MonthlyLimit { get; set; }
}

public class CreateUpdatePensionTaxReliefLimitDto
{
    public DateTime EffectiveDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MonthlyLimit { get; set; }
}

public class GetPensionTaxReliefLimitListInput : ErpPagedListInput
{
    public string Filter { get; set; }
}

public interface IPensionTaxReliefLimitAppService
    : ICrudAppService<PensionTaxReliefLimitDto, Guid, GetPensionTaxReliefLimitListInput, CreateUpdatePensionTaxReliefLimitDto, CreateUpdatePensionTaxReliefLimitDto> { }

// ---------------------------------------------------------------------------- Member history

public class MemberStatusEntryDto : EntityDto<Guid>
{
    public string MemberNo { get; set; }
    public string SchemeCode { get; set; }
    public string SponsorNo { get; set; }
    public DateTime EffectiveDate { get; set; }
    public MemberStatus FromStatus { get; set; }
    public MemberStatus ToStatus { get; set; }
    public string DocumentNo { get; set; }
    public string UserName { get; set; }
}

public class GetMemberStatusEntryListInput : ErpPagedListInput
{
    /// <summary>Matches the member number or the document number.</summary>
    public string Filter { get; set; }
    public string MemberNo { get; set; }
    public string SchemeCode { get; set; }
}

public interface IMemberStatusEntryAppService : IReadOnlyAppService<MemberStatusEntryDto, Guid, GetMemberStatusEntryListInput> { }

public class MemberSalaryEntryDto : EntityDto<Guid>
{
    public string MemberNo { get; set; }
    public string SponsorNo { get; set; }
    public DateTime Period { get; set; }
    public decimal Salary { get; set; }
    public string DocumentNo { get; set; }
}

public class GetMemberSalaryEntryListInput : ErpPagedListInput
{
    /// <summary>Matches the member number or the document number.</summary>
    public string Filter { get; set; }
    public string MemberNo { get; set; }
}

public interface IMemberSalaryEntryAppService : IReadOnlyAppService<MemberSalaryEntryDto, Guid, GetMemberSalaryEntryListInput> { }

// ---------------------------------------------------------------------------- Pension increments and pensioner history

public class PensionIncrementDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string SchemeCode { get; set; }
    public DateTime EffectiveDate { get; set; }
    public decimal IncrementPct { get; set; }
    public decimal MinimumMonthlyPension { get; set; }
    public string Description { get; set; }
    public string ReasonCode { get; set; }
    public PensionIncrementStatus Status { get; set; }
    public DateTime? AppliedDate { get; set; }
    public string AppliedBy { get; set; }
    public int NoOfPensioners { get; set; }
    public decimal TotalMonthlyIncrease { get; set; }
    public decimal TotalArrears { get; set; }
}

public class CreateUpdatePensionIncrementDto
{
    /// <summary>Blank takes the next number of the Pension Setup's Increment Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxDimensionValueCodeLength)]
    public string SchemeCode { get; set; }

    /// <summary>Any day of the first month the new pensions are paid for.</summary>
    public DateTime EffectiveDate { get; set; }

    [Range(-100, 100)]
    public decimal IncrementPct { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinimumMonthlyPension { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>A revision reason; blank gives none.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ReasonCode { get; set; }
}

public class GetPensionIncrementListInput : ErpPagedListInput
{
    /// <summary>Matches the number, the scheme or the description.</summary>
    public string Filter { get; set; }
    public string SchemeCode { get; set; }
    public PensionIncrementStatus? Status { get; set; }
}

public interface IPensionIncrementAppService
    : ICrudAppService<PensionIncrementDto, Guid, GetPensionIncrementListInput, CreateUpdatePensionIncrementDto, CreateUpdatePensionIncrementDto>
{
    /// <summary>Raises the scheme's pensions. Routed as POST /api/erp/pension-increment/{id}/apply.</summary>
    Task<PensionIncrementDto> ApplyAsync(Guid id);
}

public class PensionerChangeEntryDto : EntityDto<Guid>
{
    public string PensionerNo { get; set; }
    public string SchemeCode { get; set; }
    public PensionerChangeType ChangeType { get; set; }
    public DateTime EffectiveDate { get; set; }
    public decimal OldMonthlyPension { get; set; }
    public decimal NewMonthlyPension { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string DocumentNo { get; set; }
    public string UserName { get; set; }
}

public class GetPensionerChangeEntryListInput : ErpPagedListInput
{
    /// <summary>Matches the pensioner number or the document number.</summary>
    public string Filter { get; set; }
    public string PensionerNo { get; set; }
}

public interface IPensionerChangeEntryAppService : IReadOnlyAppService<PensionerChangeEntryDto, Guid, GetPensionerChangeEntryListInput> { }
