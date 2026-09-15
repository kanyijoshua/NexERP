using System;
using System.Collections.Generic;
using ABPmicroservice.Erp.Documents;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Purchasing;

public class VendorDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string CountryRegionCode { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public decimal Balance { get; set; }
    public string PaymentTermsCode { get; set; }
    public string VendorPostingGroup { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string CurrencyCode { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateVendorDto
{
    public string No { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string CountryRegionCode { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public string PaymentTermsCode { get; set; }
    public string VendorPostingGroup { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string CurrencyCode { get; set; }
}

public class GetVendorListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
    public bool? Blocked { get; set; }
}

public class PurchaseHeaderDto : FullAuditedEntityDto<Guid>
{
    public PurchaseDocumentType DocumentType { get; set; }
    public string No { get; set; }
    public Guid VendorId { get; set; }
    public string BuyFromVendorNo { get; set; }
    public string BuyFromVendorName { get; set; }
    public string PayToVendorNo { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ExpectedReceiptDate { get; set; }
    public DocumentStatus Status { get; set; }
    public string CurrencyCode { get; set; }
    public string PaymentTermsCode { get; set; }
    public string VendorInvoiceNo { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalAmountIncludingVat { get; set; }
    public bool Posted { get; set; }
    public string PostedDocumentNo { get; set; }
    public List<PurchaseLineDto> Lines { get; set; } = new();
}

public class PurchaseLineDto : EntityDto<Guid>
{
    public Guid PurchaseHeaderId { get; set; }
    public int LineNo { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal DirectUnitCost { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public decimal LineAmount { get; set; }
    public decimal LineAmountIncludingVat { get; set; }
    public string UnitOfMeasureCode { get; set; }
}

public class CreateUpdatePurchaseHeaderDto
{
    public PurchaseDocumentType DocumentType { get; set; }
    public string No { get; set; }
    public Guid VendorId { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ExpectedReceiptDate { get; set; }
    public string CurrencyCode { get; set; }
    public string PaymentTermsCode { get; set; }
    public string VendorInvoiceNo { get; set; }
    public List<PurchaseLineInputDto> Lines { get; set; } = new();
}

public class PurchaseLineInputDto
{
    public Guid? Id { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal DirectUnitCost { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public string UnitOfMeasureCode { get; set; }
}

public class GetPurchaseDocumentListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
    public PurchaseDocumentType? DocumentType { get; set; }
    public DocumentStatus? Status { get; set; }
    public Guid? VendorId { get; set; }
}
