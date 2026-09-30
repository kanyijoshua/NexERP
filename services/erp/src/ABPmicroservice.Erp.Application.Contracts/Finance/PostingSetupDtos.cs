using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>A posting group with nothing but a code: gen. business, gen. product, VAT and inventory.</summary>
public class PostingGroupDto : CodeTableDto { }

public class CreateUpdatePostingGroupDto : CreateUpdateCodeTableDto { }

public class CustomerPostingGroupDto : PostingGroupDto
{
    public string ReceivablesAccountNo { get; set; }
}

public class CreateUpdateCustomerPostingGroupDto : CreateUpdatePostingGroupDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ReceivablesAccountNo { get; set; }
}

public class VendorPostingGroupDto : PostingGroupDto
{
    public string PayablesAccountNo { get; set; }
}

public class CreateUpdateVendorPostingGroupDto : CreateUpdatePostingGroupDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PayablesAccountNo { get; set; }
}

public class GeneralPostingSetupDto : FullAuditedEntityDto<Guid>
{
    public string GenBusPostingGroup { get; set; }
    public string GenProdPostingGroup { get; set; }
    public string SalesAccountNo { get; set; }
    public string SalesCreditMemoAccountNo { get; set; }
    public string SalesDiscountAccountNo { get; set; }
    public string PurchAccountNo { get; set; }
    public string PurchCreditMemoAccountNo { get; set; }
    public string PurchDiscountAccountNo { get; set; }
    public string COGSAccountNo { get; set; }
    public string InventoryAdjmtAccountNo { get; set; }
}

public class CreateUpdateGeneralPostingSetupDto
{
    /// <summary>Blank is the row for customers and vendors without a business group.</summary>
    [StringLength(ErpDomainConsts.MaxGeneralBusPostingGroupLength)]
    public string GenBusPostingGroup { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxPostingGroupLength)]
    public string GenProdPostingGroup { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SalesAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SalesCreditMemoAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SalesDiscountAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PurchAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PurchCreditMemoAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PurchDiscountAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string COGSAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string InventoryAdjmtAccountNo { get; set; }
}

public class InventoryPostingSetupDto : FullAuditedEntityDto<Guid>
{
    /// <summary>Blank is the row used wherever a location has none of its own.</summary>
    public string LocationCode { get; set; }
    public string InventoryPostingGroup { get; set; }
    public string InventoryAccountNo { get; set; }
}

public class CreateUpdateInventoryPostingSetupDto
{
    [StringLength(ErpDomainConsts.MaxLocationCodeLength)]
    public string LocationCode { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxPostingGroupLength)]
    public string InventoryPostingGroup { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string InventoryAccountNo { get; set; }
}

/// <summary>General Ledger Setup (BC page 118). Blank dates mean no limit.</summary>
public class GeneralLedgerSetupDto
{
    public DateTime? AllowPostingFrom { get; set; }
    public DateTime? AllowPostingTo { get; set; }

    [StringLength(ErpDomainConsts.MaxCurrencyCodeLength)]
    public string LcyCode { get; set; }

    [Range(typeof(decimal), "0.00000001", "1000000")]
    public decimal AmountRoundingPrecision { get; set; }

    [Range(typeof(decimal), "0.00000001", "1000000")]
    public decimal UnitAmountRoundingPrecision { get; set; }

    [Range(typeof(decimal), "0.00000001", "1000000")]
    public decimal InvRoundingPrecisionLcy { get; set; }

    [StringLength(ErpDomainConsts.MaxDimensionCodeLength)]
    public string GlobalDimension1Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDimensionCodeLength)]
    public string GlobalDimension2Code { get; set; }

    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string BankAccountNos { get; set; }
}

public class VatPostingSetupDto : FullAuditedEntityDto<Guid>
{
    public string VatBusPostingGroup { get; set; }
    public string VatProdPostingGroup { get; set; }
    public string Description { get; set; }
    public string VatIdentifier { get; set; }
    public decimal VatPercent { get; set; }
    public VatCalculationType VatCalculationType { get; set; }
    public string SalesVatAccountNo { get; set; }
    public string PurchaseVatAccountNo { get; set; }
    public string ReverseChrgVatAccountNo { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateVatPostingSetupDto
{
    [StringLength(ErpDomainConsts.MaxVatBusPostingGroupLength)]
    public string VatBusPostingGroup { get; set; }

    [StringLength(ErpDomainConsts.MaxPostingGroupLength)]
    public string VatProdPostingGroup { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [StringLength(ErpDomainConsts.MaxVatIdentifierLength)]
    public string VatIdentifier { get; set; }

    [Range(0, 100)]
    public decimal VatPercent { get; set; }

    public VatCalculationType VatCalculationType { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SalesVatAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string PurchaseVatAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string ReverseChrgVatAccountNo { get; set; }

    public bool Blocked { get; set; }
}

public class VatEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime DocumentDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public VatEntryType Type { get; set; }
    public decimal Base { get; set; }
    public decimal Amount { get; set; }
    public VatCalculationType VatCalculationType { get; set; }
    public string BillToPayToNo { get; set; }
    public string VatBusPostingGroup { get; set; }
    public string VatProdPostingGroup { get; set; }
    public string VatIdentifier { get; set; }
    public decimal VatPercent { get; set; }
    public long TransactionNo { get; set; }
    public bool Closed { get; set; }
    public long ClosedByEntryNo { get; set; }
    public bool Reversed { get; set; }
}

public class GetVatEntryListInput : ErpPagedListInput
{
    /// <summary>Matches document number or customer/vendor number.</summary>
    public string Filter { get; set; }
    public VatEntryType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public interface IGenBusinessPostingGroupAppService
    : ICrudAppService<PostingGroupDto, Guid, GetCodeTableListInput, CreateUpdatePostingGroupDto, CreateUpdatePostingGroupDto> { }

public interface IGenProductPostingGroupAppService
    : ICrudAppService<PostingGroupDto, Guid, GetCodeTableListInput, CreateUpdatePostingGroupDto, CreateUpdatePostingGroupDto> { }

public interface IInventoryPostingGroupAppService
    : ICrudAppService<PostingGroupDto, Guid, GetCodeTableListInput, CreateUpdatePostingGroupDto, CreateUpdatePostingGroupDto> { }

public interface ICustomerPostingGroupAppService
    : ICrudAppService<
        CustomerPostingGroupDto,
        Guid,
        GetCodeTableListInput,
        CreateUpdateCustomerPostingGroupDto,
        CreateUpdateCustomerPostingGroupDto
    > { }

public interface IVendorPostingGroupAppService
    : ICrudAppService<
        VendorPostingGroupDto,
        Guid,
        GetCodeTableListInput,
        CreateUpdateVendorPostingGroupDto,
        CreateUpdateVendorPostingGroupDto
    > { }

public interface IGeneralPostingSetupAppService
    : ICrudAppService<
        GeneralPostingSetupDto,
        Guid,
        GetCodeTableListInput,
        CreateUpdateGeneralPostingSetupDto,
        CreateUpdateGeneralPostingSetupDto
    > { }

public interface IInventoryPostingSetupAppService
    : ICrudAppService<
        InventoryPostingSetupDto,
        Guid,
        GetCodeTableListInput,
        CreateUpdateInventoryPostingSetupDto,
        CreateUpdateInventoryPostingSetupDto
    > { }

public interface IVatBusinessPostingGroupAppService
    : ICrudAppService<PostingGroupDto, Guid, GetCodeTableListInput, CreateUpdatePostingGroupDto, CreateUpdatePostingGroupDto> { }

public interface IVatProductPostingGroupAppService
    : ICrudAppService<PostingGroupDto, Guid, GetCodeTableListInput, CreateUpdatePostingGroupDto, CreateUpdatePostingGroupDto> { }

public interface IVatPostingSetupAppService
    : ICrudAppService<VatPostingSetupDto, Guid, GetCodeTableListInput, CreateUpdateVatPostingSetupDto, CreateUpdateVatPostingSetupDto> { }

public interface IVatEntryAppService : IReadOnlyAppService<VatEntryDto, Guid, GetVatEntryListInput> { }

public interface IGeneralLedgerSetupAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/general-ledger-setup.</summary>
    Task<GeneralLedgerSetupDto> GetAsync();

    /// <summary>Routed as PUT /api/erp/general-ledger-setup.</summary>
    Task<GeneralLedgerSetupDto> UpdateAsync(GeneralLedgerSetupDto input);
}
