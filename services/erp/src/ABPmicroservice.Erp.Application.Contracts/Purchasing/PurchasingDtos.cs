using System;
using System.Collections.Generic;
using ABPmicroservice.Erp.Documents;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Purchasing;

public class VendorDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Name { get; set; }
    public string SearchName { get; set; }
    public string Name2 { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string CountryRegionCode { get; set; }
    public string PhoneNo { get; set; }
    public string MobilePhoneNo { get; set; }
    public string Email { get; set; }
    public string HomePage { get; set; }
    public string Contact { get; set; }
    public string OurAccountNo { get; set; }
    public decimal Balance { get; set; }
    public string PaymentTermsCode { get; set; }
    public string PaymentMethodCode { get; set; }
    public string ShipmentMethodCode { get; set; }
    public string ShippingAgentCode { get; set; }
    public string LocationCode { get; set; }
    public string VendorPostingGroup { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string VatBusPostingGroup { get; set; }
    public string InvoiceDiscCode { get; set; }
    public bool PricesIncludingVAT { get; set; }
    public string VATRegistrationNo { get; set; }
    public string TaxAreaCode { get; set; }
    public bool TaxLiable { get; set; }
    public bool BlockPaymentTolerance { get; set; }
    public decimal PrepaymentPct { get; set; }
    public bool AllowMultiplePostingGroups { get; set; }
    public string LeadTimeCalculation { get; set; }
    public string PurchaserCode { get; set; }
    public string CurrencyCode { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdateVendorDto
{
    public string No { get; set; }
    public string Name { get; set; }
    public string SearchName { get; set; }
    public string Name2 { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string CountryRegionCode { get; set; }
    public string PhoneNo { get; set; }
    public string MobilePhoneNo { get; set; }
    public string Email { get; set; }
    public string HomePage { get; set; }
    public string Contact { get; set; }
    public string OurAccountNo { get; set; }
    public string PaymentTermsCode { get; set; }
    public string PaymentMethodCode { get; set; }
    public string ShipmentMethodCode { get; set; }
    public string ShippingAgentCode { get; set; }
    public string LocationCode { get; set; }
    public string VendorPostingGroup { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string VatBusPostingGroup { get; set; }
    public string InvoiceDiscCode { get; set; }
    public bool PricesIncludingVAT { get; set; }
    public string VATRegistrationNo { get; set; }
    public string TaxAreaCode { get; set; }
    public bool TaxLiable { get; set; }
    public bool BlockPaymentTolerance { get; set; }
    public decimal PrepaymentPct { get; set; }
    public bool AllowMultiplePostingGroups { get; set; }
    public string LeadTimeCalculation { get; set; }
    public string PurchaserCode { get; set; }
    public string CurrencyCode { get; set; }
}

public class GetVendorListInput : ErpPagedListInput
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
    public string PayToName { get; set; }
    public string PayToAddress { get; set; }
    public string PayToCity { get; set; }
    public string PayToPostCode { get; set; }
    public string PayToCountryRegionCode { get; set; }
    public string PayToContact { get; set; }
    public string ShipToCode { get; set; }
    public string ShipToName { get; set; }
    public string ShipToAddress { get; set; }
    public string ShipToCity { get; set; }
    public string ShipToPostCode { get; set; }
    public string ShipToCountryRegionCode { get; set; }
    public string ShipToContact { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ExpectedReceiptDate { get; set; }
    public DocumentStatus Status { get; set; }
    public string CurrencyCode { get; set; }
    public string PaymentTermsCode { get; set; }
    public string PaymentMethodCode { get; set; }
    public string ShipmentMethodCode { get; set; }
    public string LocationCode { get; set; }
    public string ShortcutDimension1Code { get; set; }
    public string ShortcutDimension2Code { get; set; }
    public string VendorPostingGroup { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string VatBusPostingGroup { get; set; }
    public bool PricesIncludingVat { get; set; }
    public string OnHold { get; set; }
    public PurchaseDocumentType? AppliesToDocType { get; set; }
    public string AppliesToDocNo { get; set; }
    public string AppliesToId { get; set; }
    public string TaxAreaCode { get; set; }
    public bool TaxLiable { get; set; }
    public decimal PrepaymentPct { get; set; }
    public string YourReference { get; set; }
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
    public string LocationCode { get; set; }
    public DateTime? ExpectedReceiptDate { get; set; }
    public string ItemCategoryCode { get; set; }
    public string ShortcutDimension1Code { get; set; }
    public string ShortcutDimension2Code { get; set; }
    public decimal QtyToReceive { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QtyToInvoice { get; set; }
    public decimal QuantityInvoiced { get; set; }
    public string DeferralCode { get; set; }
    public string TaxAreaCode { get; set; }
    public bool TaxLiable { get; set; }
    public string TaxGroupCode { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string GenProdPostingGroup { get; set; }
    public string VatBusPostingGroup { get; set; }
    public string VatProdPostingGroup { get; set; }
    public ABPmicroservice.Erp.Finance.VatCalculationType VatCalculationType { get; set; }
    public string VatIdentifier { get; set; }
    public decimal VatPercent { get; set; }
    public decimal VatBaseAmount { get; set; }
    public decimal VatAmount { get; set; }
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
    public string PaymentMethodCode { get; set; }
    public string ShipmentMethodCode { get; set; }
    public string LocationCode { get; set; }
    public string ShortcutDimension1Code { get; set; }
    public string ShortcutDimension2Code { get; set; }
    public string VendorPostingGroup { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string VatBusPostingGroup { get; set; }
    public bool PricesIncludingVat { get; set; }
    public string OnHold { get; set; }
    public PurchaseDocumentType? AppliesToDocType { get; set; }
    public string AppliesToDocNo { get; set; }
    public string AppliesToId { get; set; }
    public string TaxAreaCode { get; set; }
    public bool TaxLiable { get; set; }
    public decimal PrepaymentPct { get; set; }
    public string YourReference { get; set; }
    public string VendorInvoiceNo { get; set; }
    public string PayToVendorNo { get; set; }
    public string PayToName { get; set; }
    public string PayToAddress { get; set; }
    public string PayToCity { get; set; }
    public string PayToPostCode { get; set; }
    public string PayToCountryRegionCode { get; set; }
    public string PayToContact { get; set; }
    public string ShipToCode { get; set; }
    public string ShipToName { get; set; }
    public string ShipToAddress { get; set; }
    public string ShipToCity { get; set; }
    public string ShipToPostCode { get; set; }
    public string ShipToCountryRegionCode { get; set; }
    public string ShipToContact { get; set; }
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
    public string LocationCode { get; set; }
    public DateTime? ExpectedReceiptDate { get; set; }
    public string ItemCategoryCode { get; set; }
    public string ShortcutDimension1Code { get; set; }
    public string ShortcutDimension2Code { get; set; }
    public decimal QtyToReceive { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QtyToInvoice { get; set; }
    public decimal QuantityInvoiced { get; set; }
    public string DeferralCode { get; set; }
    public string TaxAreaCode { get; set; }
    public bool TaxLiable { get; set; }
    public string TaxGroupCode { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string GenProdPostingGroup { get; set; }
}

public class GetPurchaseDocumentListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public PurchaseDocumentType? DocumentType { get; set; }
    public DocumentStatus? Status { get; set; }
    public Guid? VendorId { get; set; }
}
