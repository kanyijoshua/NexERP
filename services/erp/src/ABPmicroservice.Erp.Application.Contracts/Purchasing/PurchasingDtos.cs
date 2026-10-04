using System.ComponentModel.DataAnnotations;
using ABPmicroservice.Erp.Finance;
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

    public string County { get; set; }
    public string FaxNo { get; set; }
    public string RegistrationNumber { get; set; }
    public string GlobalDimension1Code { get; set; }
    public string GlobalDimension2Code { get; set; }
    public string LanguageCode { get; set; }
    public string PayToVendorNo { get; set; }
    public int Priority { get; set; }
    public ApplicationMethod ApplicationMethod { get; set; }
    public string ResponsibilityCenter { get; set; }
    public string PreferredBankAccountCode { get; set; }
    public string PrimaryContactNo { get; set; }
    public bool PrivacyBlocked { get; set; }
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

    [StringLength(30)]
    public string County { get; set; }

    [StringLength(30)]
    public string FaxNo { get; set; }

    [StringLength(50)]
    public string RegistrationNumber { get; set; }

    [StringLength(20)]
    public string GlobalDimension1Code { get; set; }

    [StringLength(20)]
    public string GlobalDimension2Code { get; set; }

    [StringLength(10)]
    public string LanguageCode { get; set; }

    [StringLength(20)]
    public string PayToVendorNo { get; set; }

    public int Priority { get; set; }

    public ApplicationMethod ApplicationMethod { get; set; }

    [StringLength(10)]
    public string ResponsibilityCenter { get; set; }

    [StringLength(20)]
    public string PreferredBankAccountCode { get; set; }

    [StringLength(20)]
    public string PrimaryContactNo { get; set; }

    public bool PrivacyBlocked { get; set; }
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

    public string PostingDescription { get; set; }
    public DateTime? DocumentDate { get; set; }
    public string PurchaserCode { get; set; }
    public string VendorOrderNo { get; set; }
    public string VendorShipmentNo { get; set; }
    public string VendorCrMemoNo { get; set; }
    public string BuyFromVendorName2 { get; set; }
    public string BuyFromAddress { get; set; }
    public string BuyFromAddress2 { get; set; }
    public string BuyFromCity { get; set; }
    public string BuyFromPostCode { get; set; }
    public string BuyFromCounty { get; set; }
    public string BuyFromCountryRegionCode { get; set; }
    public string BuyFromContact { get; set; }
    public string PayToName2 { get; set; }
    public string PayToAddress2 { get; set; }
    public string PayToCounty { get; set; }
    public string ShipToName2 { get; set; }
    public string ShipToAddress2 { get; set; }
    public string ShipToCounty { get; set; }
    public string OrderAddressCode { get; set; }
    public string VatRegistrationNo { get; set; }
    public decimal PaymentDiscountPct { get; set; }
    public DateTime? PmtDiscountDate { get; set; }
    public decimal VatBaseDiscountPct { get; set; }
    public string LanguageCode { get; set; }
    public string QuoteNo { get; set; }
    public string ResponsibilityCenter { get; set; }
    public DateTime? RequestedReceiptDate { get; set; }
    public DateTime? PromisedReceiptDate { get; set; }
    public string AssignedUserId { get; set; }
    public string PaymentReference { get; set; }
    public DateTime? InvoiceReceivedDate { get; set; }
    public string CreditorNo { get; set; }
    public string ReasonCode { get; set; }
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

    [StringLength(100)]
    public string PostingDescription { get; set; }

    public DateTime? DocumentDate { get; set; }

    [StringLength(20)]
    public string PurchaserCode { get; set; }

    [StringLength(35)]
    public string VendorOrderNo { get; set; }

    [StringLength(35)]
    public string VendorShipmentNo { get; set; }

    [StringLength(35)]
    public string VendorCrMemoNo { get; set; }

    [StringLength(50)]
    public string BuyFromVendorName2 { get; set; }

    [StringLength(100)]
    public string BuyFromAddress { get; set; }

    [StringLength(50)]
    public string BuyFromAddress2 { get; set; }

    [StringLength(30)]
    public string BuyFromCity { get; set; }

    [StringLength(20)]
    public string BuyFromPostCode { get; set; }

    [StringLength(30)]
    public string BuyFromCounty { get; set; }

    [StringLength(10)]
    public string BuyFromCountryRegionCode { get; set; }

    [StringLength(100)]
    public string BuyFromContact { get; set; }

    [StringLength(50)]
    public string PayToName2 { get; set; }

    [StringLength(50)]
    public string PayToAddress2 { get; set; }

    [StringLength(30)]
    public string PayToCounty { get; set; }

    [StringLength(50)]
    public string ShipToName2 { get; set; }

    [StringLength(50)]
    public string ShipToAddress2 { get; set; }

    [StringLength(30)]
    public string ShipToCounty { get; set; }

    [StringLength(10)]
    public string OrderAddressCode { get; set; }

    [StringLength(20)]
    public string VatRegistrationNo { get; set; }

    public decimal PaymentDiscountPct { get; set; }

    public DateTime? PmtDiscountDate { get; set; }

    public decimal VatBaseDiscountPct { get; set; }

    [StringLength(10)]
    public string LanguageCode { get; set; }

    [StringLength(20)]
    public string QuoteNo { get; set; }

    [StringLength(10)]
    public string ResponsibilityCenter { get; set; }

    public DateTime? RequestedReceiptDate { get; set; }

    public DateTime? PromisedReceiptDate { get; set; }

    [StringLength(50)]
    public string AssignedUserId { get; set; }

    [StringLength(50)]
    public string PaymentReference { get; set; }

    public DateTime? InvoiceReceivedDate { get; set; }

    [StringLength(20)]
    public string CreditorNo { get; set; }

    [StringLength(10)]
    public string ReasonCode { get; set; }
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
