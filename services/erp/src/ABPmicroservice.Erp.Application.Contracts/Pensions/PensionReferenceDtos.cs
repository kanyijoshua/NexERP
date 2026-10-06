using System;
using System.ComponentModel.DataAnnotations;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Pensions;

// ---------------------------------------------------------------------------- Banks and branches

public class PensionBankDto : CodeTableDto
{
    public string SwiftCode { get; set; }
}

public class CreateUpdatePensionBankDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxSwiftCodeLength)]
    public string SwiftCode { get; set; }
}

public interface IPensionBankAppService
    : ICrudAppService<PensionBankDto, Guid, GetCodeTableListInput, CreateUpdatePensionBankDto, CreateUpdatePensionBankDto> { }

public class PensionBankBranchDto : FullAuditedEntityDto<Guid>
{
    public string BankCode { get; set; }
    public string BranchCode { get; set; }
    public string Name { get; set; }
    public string SwiftCode { get; set; }
}

public class CreateUpdatePensionBankBranchDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BankCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BranchCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxSwiftCodeLength)]
    public string SwiftCode { get; set; }
}

public class GetPensionBankBranchListInput : ErpPagedListInput
{
    /// <summary>Matches the branch code or name.</summary>
    public string Filter { get; set; }
    public string BankCode { get; set; }
}

public interface IPensionBankBranchAppService
    : ICrudAppService<PensionBankBranchDto, Guid, GetPensionBankBranchListInput, CreateUpdatePensionBankBranchDto, CreateUpdatePensionBankBranchDto> { }

// ---------------------------------------------------------------------------- Pay modes and reasons

public class PensionerPayModeDto : CodeTableDto
{
    public PensionerPaymentType PaymentType { get; set; }
}

public class CreateUpdatePensionerPayModeDto : CreateUpdateCodeTableDto
{
    public PensionerPaymentType PaymentType { get; set; }
}

public interface IPensionerPayModeAppService
    : ICrudAppService<PensionerPayModeDto, Guid, GetCodeTableListInput, CreateUpdatePensionerPayModeDto, CreateUpdatePensionerPayModeDto> { }

public class PensionerSuspensionReasonDto : CodeTableDto
{
    public bool LifeCertificate { get; set; }
}

public class CreateUpdatePensionerSuspensionReasonDto : CreateUpdateCodeTableDto
{
    /// <summary>The reason for pensions stopped because no life certificate came in.</summary>
    public bool LifeCertificate { get; set; }
}

public interface IPensionerSuspensionReasonAppService
    : ICrudAppService<PensionerSuspensionReasonDto, Guid, GetCodeTableListInput, CreateUpdatePensionerSuspensionReasonDto, CreateUpdatePensionerSuspensionReasonDto> { }

public interface IPensionRevisionReasonAppService
    : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

// ---------------------------------------------------------------------------- Other schemes

public class OtherPensionSchemeDto : CodeTableDto
{
    public string RegulatorReferenceNo { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ContactName { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public string BankCode { get; set; }
    public string BankBranchCode { get; set; }
    public string BankAccountNo { get; set; }
}

public class CreateUpdateOtherPensionSchemeDto : CreateUpdateCodeTableDto
{
    [StringLength(ErpDomainConsts.MaxExternalDocumentNoLength)]
    public string RegulatorReferenceNo { get; set; }

    [StringLength(ErpDomainConsts.MaxAddressLength)]
    public string Address { get; set; }

    [StringLength(ErpDomainConsts.MaxCityLength)]
    public string City { get; set; }

    [StringLength(ErpDomainConsts.MaxContactLength)]
    public string ContactName { get; set; }

    [StringLength(ErpDomainConsts.MaxPhoneLength)]
    public string PhoneNo { get; set; }

    [StringLength(ErpDomainConsts.MaxEmailLength)]
    public string Email { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BankCode { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string BankBranchCode { get; set; }

    [StringLength(ErpDomainConsts.MaxBankAccountNoLength)]
    public string BankAccountNo { get; set; }
}

public interface IOtherPensionSchemeAppService
    : ICrudAppService<OtherPensionSchemeDto, Guid, GetCodeTableListInput, CreateUpdateOtherPensionSchemeDto, CreateUpdateOtherPensionSchemeDto> { }

// ---------------------------------------------------------------------------- Pensioner earnings and deductions

public class PensionerPayItemDto : CodeTableDto
{
    public PensionerPayItemType ItemType { get; set; }
    public PensionerPayItemCalculation Calculation { get; set; }
    public decimal Amount { get; set; }
    public decimal Pct { get; set; }
    public bool Taxable { get; set; }
    public string AccountNo { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdatePensionerPayItemDto : CreateUpdateCodeTableDto
{
    public PensionerPayItemType ItemType { get; set; }
    public PensionerPayItemCalculation Calculation { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    [Range(0, 100)]
    public decimal Pct { get; set; }

    /// <summary>An earning taxed with the pension, or a deduction taken off before tax.</summary>
    public bool Taxable { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string AccountNo { get; set; }

    public bool Blocked { get; set; }
}

public interface IPensionerPayItemAppService
    : ICrudAppService<PensionerPayItemDto, Guid, GetCodeTableListInput, CreateUpdatePensionerPayItemDto, CreateUpdatePensionerPayItemDto> { }

public class PensionerPayItemAssignmentDto : FullAuditedEntityDto<Guid>
{
    public string PensionerNo { get; set; }
    public string PayItemCode { get; set; }
    public string PayItemDescription { get; set; }
    public PensionerPayItemType ItemType { get; set; }
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Comment { get; set; }
}

public class CreateUpdatePensionerPayItemAssignmentDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PensionerNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string PayItemCode { get; set; }

    /// <summary>The pensioner's own amount, or percentage for a percentage item; zero takes the item's.</summary>
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    /// <summary>Blank starts this month.</summary>
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Comment { get; set; }
}

public class GetPensionerPayItemAssignmentListInput : ErpPagedListInput
{
    /// <summary>Matches the pensioner number or the item code.</summary>
    public string Filter { get; set; }
    public string PensionerNo { get; set; }
}

public interface IPensionerPayItemAssignmentAppService
    : ICrudAppService<PensionerPayItemAssignmentDto, Guid, GetPensionerPayItemAssignmentListInput, CreateUpdatePensionerPayItemAssignmentDto, CreateUpdatePensionerPayItemAssignmentDto> { }

public class PensionPayrollLineItemDto : EntityDto<Guid>
{
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string PensionerNo { get; set; }
    public string PayItemCode { get; set; }
    public string Description { get; set; }
    public PensionerPayItemType ItemType { get; set; }
    public bool Taxable { get; set; }
    public string AccountNo { get; set; }
    public decimal Amount { get; set; }
}

public class GetPensionPayrollLineItemListInput : ErpPagedListInput
{
    /// <summary>Matches the pensioner number or the item code.</summary>
    public string Filter { get; set; }
    public string DocumentNo { get; set; }
    public int? LineNo { get; set; }
    public string PensionerNo { get; set; }
}

public interface IPensionPayrollLineItemAppService : IReadOnlyAppService<PensionPayrollLineItemDto, Guid, GetPensionPayrollLineItemListInput> { }

// ---------------------------------------------------------------------------- Exit documents

public class ExitReasonDocumentDto : FullAuditedEntityDto<Guid>
{
    public string ExitReasonCode { get; set; }
    public int LineNo { get; set; }
    public string DocumentName { get; set; }
    public bool Mandatory { get; set; }
}

public class CreateUpdateExitReasonDocumentDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ExitReasonCode { get; set; }

    /// <summary>0 puts the document after the last one.</summary>
    public int LineNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string DocumentName { get; set; }

    public bool Mandatory { get; set; } = true;
}

public class GetExitReasonDocumentListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string ExitReasonCode { get; set; }
}

public interface IExitReasonDocumentAppService
    : ICrudAppService<ExitReasonDocumentDto, Guid, GetExitReasonDocumentListInput, CreateUpdateExitReasonDocumentDto, CreateUpdateExitReasonDocumentDto> { }

public class MemberExitDocumentDto : FullAuditedEntityDto<Guid>
{
    public string ExitNo { get; set; }
    public int LineNo { get; set; }
    public string DocumentName { get; set; }
    public bool Mandatory { get; set; }
    public bool Received { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string Remarks { get; set; }
}

public class CreateUpdateMemberExitDocumentDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string ExitNo { get; set; }

    /// <summary>0 puts the document after the last one.</summary>
    public int LineNo { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string DocumentName { get; set; }

    public bool Mandatory { get; set; }
    public bool Received { get; set; }

    /// <summary>Blank on a received document is today.</summary>
    public DateTime? ReceivedDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Remarks { get; set; }
}

public class GetMemberExitDocumentListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string ExitNo { get; set; }
}

public interface IMemberExitDocumentAppService
    : ICrudAppService<MemberExitDocumentDto, Guid, GetMemberExitDocumentListInput, CreateUpdateMemberExitDocumentDto, CreateUpdateMemberExitDocumentDto> { }

// ---------------------------------------------------------------------------- Age factors

public class PensionAgeFactorDto : FullAuditedEntityDto<Guid>
{
    public string SchemeCode { get; set; }
    public PensionFactorType FactorType { get; set; }
    public int Age { get; set; }
    public decimal MaleFactor { get; set; }
    public decimal FemaleFactor { get; set; }
}

public class CreateUpdatePensionAgeFactorDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxDimensionValueCodeLength)]
    public string SchemeCode { get; set; }

    public PensionFactorType FactorType { get; set; }

    [Range(0, 120)]
    public int Age { get; set; }

    [Range(0, 1000)]
    public decimal MaleFactor { get; set; }

    [Range(0, 1000)]
    public decimal FemaleFactor { get; set; }
}

public class GetPensionAgeFactorListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string SchemeCode { get; set; }
    public PensionFactorType? FactorType { get; set; }
}

public interface IPensionAgeFactorAppService
    : ICrudAppService<PensionAgeFactorDto, Guid, GetPensionAgeFactorListInput, CreateUpdatePensionAgeFactorDto, CreateUpdatePensionAgeFactorDto> { }

public class CopyExitDocumentsResultDto
{
    public int NoOfDocuments { get; set; }
}
