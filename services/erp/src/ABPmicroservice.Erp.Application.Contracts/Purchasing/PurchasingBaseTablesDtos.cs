using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Purchasing;

public class VendorBankAccountDto : FullAuditedEntityDto<Guid>
{
    public string VendorNo { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Name2 { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string Contact { get; set; }
    public string PhoneNo { get; set; }
    public string BankBranchNo { get; set; }
    public string BankAccountNo { get; set; }
    public string TransitNo { get; set; }
    public string CurrencyCode { get; set; }
    public string CountryRegionCode { get; set; }
    public string County { get; set; }
    public string FaxNo { get; set; }
    public string Email { get; set; }
    public string Iban { get; set; }
    public string SwiftCode { get; set; }
    public string BankClearingCode { get; set; }
    public string BankClearingStandard { get; set; }
}

public class CreateUpdateVendorBankAccountDto
{
    [Required]
    [StringLength(20)]
    public string VendorNo { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(50)]
    public string Name2 { get; set; }

    [StringLength(100)]
    public string Address { get; set; }

    [StringLength(50)]
    public string Address2 { get; set; }

    [StringLength(30)]
    public string City { get; set; }

    [StringLength(20)]
    public string PostCode { get; set; }

    [StringLength(100)]
    public string Contact { get; set; }

    [StringLength(30)]
    public string PhoneNo { get; set; }

    [StringLength(20)]
    public string BankBranchNo { get; set; }

    [StringLength(30)]
    public string BankAccountNo { get; set; }

    [StringLength(20)]
    public string TransitNo { get; set; }

    [StringLength(10)]
    public string CurrencyCode { get; set; }

    [StringLength(10)]
    public string CountryRegionCode { get; set; }

    [StringLength(30)]
    public string County { get; set; }

    [StringLength(30)]
    public string FaxNo { get; set; }

    [StringLength(80)]
    public string Email { get; set; }

    [StringLength(50)]
    public string Iban { get; set; }

    [StringLength(20)]
    public string SwiftCode { get; set; }

    [StringLength(50)]
    public string BankClearingCode { get; set; }

    [StringLength(50)]
    public string BankClearingStandard { get; set; }
}

public class GetVendorBankAccountListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string VendorNo { get; set; }
}

/// <summary>Vendor Bank Accounts.</summary>
public interface IVendorBankAccountAppService : ICrudAppService<VendorBankAccountDto, Guid, GetVendorBankAccountListInput, CreateUpdateVendorBankAccountDto, CreateUpdateVendorBankAccountDto> { }

public class DetailedVendorLedgEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public long VendorLedgerEntryNo { get; set; }
    public DetailedCVLedgerEntryType EntryType { get; set; }
    public DateTime? PostingDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountLcy { get; set; }
    public string VendorNo { get; set; }
    public string CurrencyCode { get; set; }
    public string UserId { get; set; }
    public string SourceCode { get; set; }
    public long TransactionNo { get; set; }
    public string JournalBatchName { get; set; }
    public string ReasonCode { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal DebitAmountLcy { get; set; }
    public decimal CreditAmountLcy { get; set; }
    public DateTime? InitialEntryDueDate { get; set; }
    public string InitialEntryGlobalDim1 { get; set; }
    public string InitialEntryGlobalDim2 { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string GenProdPostingGroup { get; set; }
    public string VatBusPostingGroup { get; set; }
    public string VatProdPostingGroup { get; set; }
    public GLEntryDocumentType InitialDocumentType { get; set; }
    public long AppliedVendLedgerEntryNo { get; set; }
    public bool Unapplied { get; set; }
    public long UnappliedByEntryNo { get; set; }
    public decimal RemainingPmtDiscPossible { get; set; }
    public decimal MaxPaymentTolerance { get; set; }
    public int ApplicationNo { get; set; }
    public bool LedgerEntryAmount { get; set; }
    public string PostingGroup { get; set; }
    public int ExchRateAdjmtRegNo { get; set; }
}

public class GetDetailedVendorLedgEntryListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string VendorNo { get; set; }
    public string DocumentNo { get; set; }
}

/// <summary>Detailed Vendor Ledg. Entries.</summary>
public interface IDetailedVendorLedgEntryAppService : IReadOnlyAppService<DetailedVendorLedgEntryDto, Guid, GetDetailedVendorLedgEntryListInput> { }

public class OrderAddressDto : FullAuditedEntityDto<Guid>
{
    public string VendorNo { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Name2 { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string Contact { get; set; }
    public string PhoneNo { get; set; }
    public string CountryRegionCode { get; set; }
    public string FaxNo { get; set; }
    public string PostCode { get; set; }
    public string County { get; set; }
    public string Email { get; set; }
}

public class CreateUpdateOrderAddressDto
{
    [Required]
    [StringLength(20)]
    public string VendorNo { get; set; }

    [Required]
    [StringLength(10)]
    public string Code { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(50)]
    public string Name2 { get; set; }

    [StringLength(100)]
    public string Address { get; set; }

    [StringLength(50)]
    public string Address2 { get; set; }

    [StringLength(30)]
    public string City { get; set; }

    [StringLength(100)]
    public string Contact { get; set; }

    [StringLength(30)]
    public string PhoneNo { get; set; }

    [StringLength(10)]
    public string CountryRegionCode { get; set; }

    [StringLength(30)]
    public string FaxNo { get; set; }

    [StringLength(20)]
    public string PostCode { get; set; }

    [StringLength(30)]
    public string County { get; set; }

    [StringLength(80)]
    public string Email { get; set; }
}

public class GetOrderAddressListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string VendorNo { get; set; }
}

/// <summary>Order Addresses.</summary>
public interface IOrderAddressAppService : ICrudAppService<OrderAddressDto, Guid, GetOrderAddressListInput, CreateUpdateOrderAddressDto, CreateUpdateOrderAddressDto> { }

public class ItemVendorDto : FullAuditedEntityDto<Guid>
{
    public string VendorNo { get; set; }
    public string ItemNo { get; set; }
    public string LeadTimeCalculation { get; set; }
    public string VendorItemNo { get; set; }
}

public class CreateUpdateItemVendorDto
{
    [Required]
    [StringLength(20)]
    public string VendorNo { get; set; }

    [Required]
    [StringLength(20)]
    public string ItemNo { get; set; }

    [StringLength(32)]
    public string LeadTimeCalculation { get; set; }

    [StringLength(50)]
    public string VendorItemNo { get; set; }
}

public class GetItemVendorListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string VendorNo { get; set; }
    public string ItemNo { get; set; }
}

/// <summary>Item Vendor Catalog.</summary>
public interface IItemVendorAppService : ICrudAppService<ItemVendorDto, Guid, GetItemVendorListInput, CreateUpdateItemVendorDto, CreateUpdateItemVendorDto> { }

public class PurchCommentLineDto : FullAuditedEntityDto<Guid>
{
    public PurchaseCommentDocumentType DocumentType { get; set; }
    public string No { get; set; }
    public int DocumentLineNo { get; set; }
    public int LineNo { get; set; }
    public DateTime? Date { get; set; }
    public string Code { get; set; }
    public string Comment { get; set; }
}

public class CreateUpdatePurchCommentLineDto
{
    public PurchaseCommentDocumentType DocumentType { get; set; }

    [Required]
    [StringLength(20)]
    public string No { get; set; }

    public int DocumentLineNo { get; set; }

    /// <summary>Zero takes the next free number.</summary>
    public int LineNo { get; set; }

    public DateTime? Date { get; set; }

    [StringLength(10)]
    public string Code { get; set; }

    [StringLength(80)]
    public string Comment { get; set; }
}

public class GetPurchCommentLineListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string No { get; set; }
}

/// <summary>Purchase Comment Lines.</summary>
public interface IPurchCommentLineAppService : ICrudAppService<PurchCommentLineDto, Guid, GetPurchCommentLineListInput, CreateUpdatePurchCommentLineDto, CreateUpdatePurchCommentLineDto> { }
