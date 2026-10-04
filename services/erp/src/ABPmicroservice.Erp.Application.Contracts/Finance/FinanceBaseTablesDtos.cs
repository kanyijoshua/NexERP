using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>Source Codes.</summary>
public interface ISourceCodeAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

/// <summary>Reason Codes.</summary>
public interface IReasonCodeAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

public class CountryRegionDto : CodeTableDto
{
    public string IsoCode { get; set; }
    public string IsoNumericCode { get; set; }
    public string EuCountryRegionCode { get; set; }
    public string IntrastatCode { get; set; }
    public CountryRegionAddressFormat AddressFormat { get; set; }
    public CountryRegionContactAddressFormat ContactAddressFormat { get; set; }
    public string VatScheme { get; set; }
    public string CountyName { get; set; }
}

public class CreateUpdateCountryRegionDto : CreateUpdateCodeTableDto
{
    [StringLength(2)]
    public string IsoCode { get; set; }

    [StringLength(3)]
    public string IsoNumericCode { get; set; }

    [StringLength(10)]
    public string EuCountryRegionCode { get; set; }

    [StringLength(10)]
    public string IntrastatCode { get; set; }

    public CountryRegionAddressFormat AddressFormat { get; set; }

    public CountryRegionContactAddressFormat ContactAddressFormat { get; set; }

    [StringLength(10)]
    public string VatScheme { get; set; }

    [StringLength(30)]
    public string CountyName { get; set; }
}

/// <summary>Countries/Regions.</summary>
public interface ICountryRegionAppService : ICrudAppService<CountryRegionDto, Guid, GetCodeTableListInput, CreateUpdateCountryRegionDto, CreateUpdateCountryRegionDto> { }

public class PostCodeDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string City { get; set; }
    public string SearchCity { get; set; }
    public string CountryRegionCode { get; set; }
    public string County { get; set; }
}

public class CreateUpdatePostCodeDto
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; }

    [Required]
    [StringLength(30)]
    public string City { get; set; }

    [StringLength(30)]
    public string SearchCity { get; set; }

    [StringLength(10)]
    public string CountryRegionCode { get; set; }

    [StringLength(30)]
    public string County { get; set; }
}

public class GetPostCodeListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
}

/// <summary>Post Codes.</summary>
public interface IPostCodeAppService : ICrudAppService<PostCodeDto, Guid, GetPostCodeListInput, CreateUpdatePostCodeDto, CreateUpdatePostCodeDto> { }

/// <summary>Shipment Methods.</summary>
public interface IShipmentMethodAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

public class ResponsibilityCenterDto : CodeTableDto
{
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string CountryRegionCode { get; set; }
    public string PhoneNo { get; set; }
    public string FaxNo { get; set; }
    public string Name2 { get; set; }
    public string Contact { get; set; }
    public string GlobalDimension1Code { get; set; }
    public string GlobalDimension2Code { get; set; }
    public string LocationCode { get; set; }
    public string County { get; set; }
    public string Email { get; set; }
}

public class CreateUpdateResponsibilityCenterDto : CreateUpdateCodeTableDto
{
    [StringLength(100)]
    public string Address { get; set; }

    [StringLength(50)]
    public string Address2 { get; set; }

    [StringLength(30)]
    public string City { get; set; }

    [StringLength(20)]
    public string PostCode { get; set; }

    [StringLength(10)]
    public string CountryRegionCode { get; set; }

    [StringLength(30)]
    public string PhoneNo { get; set; }

    [StringLength(30)]
    public string FaxNo { get; set; }

    [StringLength(50)]
    public string Name2 { get; set; }

    [StringLength(100)]
    public string Contact { get; set; }

    [StringLength(20)]
    public string GlobalDimension1Code { get; set; }

    [StringLength(20)]
    public string GlobalDimension2Code { get; set; }

    [StringLength(10)]
    public string LocationCode { get; set; }

    [StringLength(30)]
    public string County { get; set; }

    [StringLength(80)]
    public string Email { get; set; }
}

/// <summary>Responsibility Centers.</summary>
public interface IResponsibilityCenterAppService : ICrudAppService<ResponsibilityCenterDto, Guid, GetCodeTableListInput, CreateUpdateResponsibilityCenterDto, CreateUpdateResponsibilityCenterDto> { }

public class UserSetupDto : FullAuditedEntityDto<Guid>
{
    public string UserId { get; set; }
    public DateTime? AllowPostingFrom { get; set; }
    public DateTime? AllowPostingTo { get; set; }
    public bool RegisterTime { get; set; }
    public DateTime? AllowDeferralPostingFrom { get; set; }
    public DateTime? AllowDeferralPostingTo { get; set; }
    public string SalespersPurchCode { get; set; }
    public string ApproverId { get; set; }
    public int SalesAmountApprovalLimit { get; set; }
    public int PurchaseAmountApprovalLimit { get; set; }
    public bool UnlimitedSalesApproval { get; set; }
    public bool UnlimitedPurchaseApproval { get; set; }
    public string Substitute { get; set; }
    public string Email { get; set; }
    public string PhoneNo { get; set; }
    public int RequestAmountApprovalLimit { get; set; }
    public bool UnlimitedRequestApproval { get; set; }
    public bool ApprovalAdministrator { get; set; }
    public DateTime? AllowVatDateFrom { get; set; }
    public DateTime? AllowVatDateTo { get; set; }
    public InvoicePostingPolicy SalesInvoicePostingPolicy { get; set; }
    public InvoicePostingPolicy PurchInvoicePostingPolicy { get; set; }
    public DateTime? AllowFAPostingFrom { get; set; }
    public DateTime? AllowFAPostingTo { get; set; }
    public string SalesRespCtrFilter { get; set; }
    public string PurchaseRespCtrFilter { get; set; }
}

public class CreateUpdateUserSetupDto
{
    [Required]
    [StringLength(50)]
    public string UserId { get; set; }

    public DateTime? AllowPostingFrom { get; set; }

    public DateTime? AllowPostingTo { get; set; }

    public bool RegisterTime { get; set; }

    public DateTime? AllowDeferralPostingFrom { get; set; }

    public DateTime? AllowDeferralPostingTo { get; set; }

    [StringLength(20)]
    public string SalespersPurchCode { get; set; }

    [StringLength(50)]
    public string ApproverId { get; set; }

    public int SalesAmountApprovalLimit { get; set; }

    public int PurchaseAmountApprovalLimit { get; set; }

    public bool UnlimitedSalesApproval { get; set; }

    public bool UnlimitedPurchaseApproval { get; set; }

    [StringLength(50)]
    public string Substitute { get; set; }

    [StringLength(100)]
    public string Email { get; set; }

    [StringLength(30)]
    public string PhoneNo { get; set; }

    public int RequestAmountApprovalLimit { get; set; }

    public bool UnlimitedRequestApproval { get; set; }

    public bool ApprovalAdministrator { get; set; }

    public DateTime? AllowVatDateFrom { get; set; }

    public DateTime? AllowVatDateTo { get; set; }

    public InvoicePostingPolicy SalesInvoicePostingPolicy { get; set; }

    public InvoicePostingPolicy PurchInvoicePostingPolicy { get; set; }

    public DateTime? AllowFAPostingFrom { get; set; }

    public DateTime? AllowFAPostingTo { get; set; }

    [StringLength(10)]
    public string SalesRespCtrFilter { get; set; }

    [StringLength(10)]
    public string PurchaseRespCtrFilter { get; set; }
}

public class GetUserSetupListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
}

/// <summary>User Setup.</summary>
public interface IUserSetupAppService : ICrudAppService<UserSetupDto, Guid, GetUserSetupListInput, CreateUpdateUserSetupDto, CreateUpdateUserSetupDto> { }

public class GLBudgetNameDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Blocked { get; set; }
    public string BudgetDimension1Code { get; set; }
    public string BudgetDimension2Code { get; set; }
    public string BudgetDimension3Code { get; set; }
    public string BudgetDimension4Code { get; set; }
}

public class CreateUpdateGLBudgetNameDto
{
    [Required]
    [StringLength(10)]
    public string Name { get; set; }

    [StringLength(80)]
    public string Description { get; set; }

    public bool Blocked { get; set; }

    [StringLength(20)]
    public string BudgetDimension1Code { get; set; }

    [StringLength(20)]
    public string BudgetDimension2Code { get; set; }

    [StringLength(20)]
    public string BudgetDimension3Code { get; set; }

    [StringLength(20)]
    public string BudgetDimension4Code { get; set; }
}

public class GetGLBudgetNameListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
}

/// <summary>G/L Budgets.</summary>
public interface IGLBudgetNameAppService : ICrudAppService<GLBudgetNameDto, Guid, GetGLBudgetNameListInput, CreateUpdateGLBudgetNameDto, CreateUpdateGLBudgetNameDto> { }

public class GLBudgetEntryDto : FullAuditedEntityDto<Guid>
{
    public long EntryNo { get; set; }
    public string BudgetName { get; set; }
    public string GLAccountNo { get; set; }
    public DateTime? Date { get; set; }
    public string GlobalDimension1Code { get; set; }
    public string GlobalDimension2Code { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string BusinessUnitCode { get; set; }
    public string BudgetDimension1Code { get; set; }
    public string BudgetDimension2Code { get; set; }
    public string BudgetDimension3Code { get; set; }
    public string BudgetDimension4Code { get; set; }
}

public class CreateUpdateGLBudgetEntryDto
{
    /// <summary>Zero takes the next free number.</summary>
    public long EntryNo { get; set; }

    [Required]
    [StringLength(10)]
    public string BudgetName { get; set; }

    [Required]
    [StringLength(20)]
    public string GLAccountNo { get; set; }

    public DateTime? Date { get; set; }

    [StringLength(20)]
    public string GlobalDimension1Code { get; set; }

    [StringLength(20)]
    public string GlobalDimension2Code { get; set; }

    public decimal Amount { get; set; }

    [StringLength(100)]
    public string Description { get; set; }

    [StringLength(20)]
    public string BusinessUnitCode { get; set; }

    [StringLength(20)]
    public string BudgetDimension1Code { get; set; }

    [StringLength(20)]
    public string BudgetDimension2Code { get; set; }

    [StringLength(20)]
    public string BudgetDimension3Code { get; set; }

    [StringLength(20)]
    public string BudgetDimension4Code { get; set; }
}

public class GetGLBudgetEntryListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string BudgetName { get; set; }
    public string GLAccountNo { get; set; }
}

/// <summary>G/L Budget Entries.</summary>
public interface IGLBudgetEntryAppService : ICrudAppService<GLBudgetEntryDto, Guid, GetGLBudgetEntryListInput, CreateUpdateGLBudgetEntryDto, CreateUpdateGLBudgetEntryDto> { }

public class CommentLineDto : FullAuditedEntityDto<Guid>
{
    public CommentLineTableName TableName { get; set; }
    public string No { get; set; }
    public int LineNo { get; set; }
    public DateTime? Date { get; set; }
    public string Code { get; set; }
    public string Comment { get; set; }
}

public class CreateUpdateCommentLineDto
{
    public CommentLineTableName TableName { get; set; }

    [Required]
    [StringLength(20)]
    public string No { get; set; }

    /// <summary>Zero takes the next free number.</summary>
    public int LineNo { get; set; }

    public DateTime? Date { get; set; }

    [StringLength(10)]
    public string Code { get; set; }

    [StringLength(80)]
    public string Comment { get; set; }
}

public class GetCommentLineListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string No { get; set; }
}

/// <summary>Comment Lines.</summary>
public interface ICommentLineAppService : ICrudAppService<CommentLineDto, Guid, GetCommentLineListInput, CreateUpdateCommentLineDto, CreateUpdateCommentLineDto> { }
