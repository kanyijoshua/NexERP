using System.ComponentModel.DataAnnotations;
using ABPmicroservice.Erp.Finance;
using System;
using System.Collections.Generic;
using ABPmicroservice.Erp.Documents;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Sales;

public class CustomerDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string CountryRegionCode { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal Balance { get; set; }
    public string PaymentTermsCode { get; set; }
    public string CustomerPostingGroup { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string VatBusPostingGroup { get; set; }
    public string SalespersonCode { get; set; }
    public string PaymentMethodCode { get; set; }
    public string CurrencyCode { get; set; }
    public bool Blocked { get; set; }

    public string SearchName { get; set; }
    public string Name2 { get; set; }
    public string Address2 { get; set; }
    public string County { get; set; }
    public string Contact { get; set; }
    public string MobilePhoneNo { get; set; }
    public string HomePage { get; set; }
    public string VatRegistrationNo { get; set; }
    public string RegistrationNumber { get; set; }
    public string GlobalDimension1Code { get; set; }
    public string GlobalDimension2Code { get; set; }
    public string LanguageCode { get; set; }
    public string LocationCode { get; set; }
    public string ShipmentMethodCode { get; set; }
    public string ResponsibilityCenter { get; set; }
    public string CustomerPriceGroup { get; set; }
    public string CustomerDiscGroup { get; set; }
    public string InvoiceDiscCode { get; set; }
    public string FinChargeTermsCode { get; set; }
    public string ReminderTermsCode { get; set; }
    public ApplicationMethod ApplicationMethod { get; set; }
    public bool PricesIncludingVat { get; set; }
    public string TaxAreaCode { get; set; }
    public bool TaxLiable { get; set; }
    public bool BlockPaymentTolerance { get; set; }
    public decimal PrepaymentPct { get; set; }
    public bool PrintStatements { get; set; }
    public int LastStatementNo { get; set; }
    public bool CombineShipments { get; set; }
    public string PreferredBankAccountCode { get; set; }
    public string PrimaryContactNo { get; set; }
    public bool PrivacyBlocked { get; set; }
}

public class CreateUpdateCustomerDto
{
    public string No { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
    public string CountryRegionCode { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public decimal CreditLimit { get; set; }
    public string PaymentTermsCode { get; set; }
    public string CustomerPostingGroup { get; set; }
    public string GenBusPostingGroup { get; set; }
    public string VatBusPostingGroup { get; set; }
    public string SalespersonCode { get; set; }
    public string PaymentMethodCode { get; set; }
    public string CurrencyCode { get; set; }

    [StringLength(100)]
    public string SearchName { get; set; }

    [StringLength(50)]
    public string Name2 { get; set; }

    [StringLength(50)]
    public string Address2 { get; set; }

    [StringLength(30)]
    public string County { get; set; }

    [StringLength(100)]
    public string Contact { get; set; }

    [StringLength(30)]
    public string MobilePhoneNo { get; set; }

    [StringLength(80)]
    public string HomePage { get; set; }

    [StringLength(20)]
    public string VatRegistrationNo { get; set; }

    [StringLength(50)]
    public string RegistrationNumber { get; set; }

    [StringLength(20)]
    public string GlobalDimension1Code { get; set; }

    [StringLength(20)]
    public string GlobalDimension2Code { get; set; }

    [StringLength(10)]
    public string LanguageCode { get; set; }

    [StringLength(10)]
    public string LocationCode { get; set; }

    [StringLength(10)]
    public string ShipmentMethodCode { get; set; }

    [StringLength(10)]
    public string ResponsibilityCenter { get; set; }

    [StringLength(10)]
    public string CustomerPriceGroup { get; set; }

    [StringLength(20)]
    public string CustomerDiscGroup { get; set; }

    [StringLength(20)]
    public string InvoiceDiscCode { get; set; }

    [StringLength(10)]
    public string FinChargeTermsCode { get; set; }

    [StringLength(10)]
    public string ReminderTermsCode { get; set; }

    public ApplicationMethod ApplicationMethod { get; set; }

    public bool PricesIncludingVat { get; set; }

    [StringLength(20)]
    public string TaxAreaCode { get; set; }

    public bool TaxLiable { get; set; }

    public bool BlockPaymentTolerance { get; set; }

    public decimal PrepaymentPct { get; set; }

    public bool PrintStatements { get; set; }

    public int LastStatementNo { get; set; }

    public bool CombineShipments { get; set; }

    [StringLength(20)]
    public string PreferredBankAccountCode { get; set; }

    [StringLength(20)]
    public string PrimaryContactNo { get; set; }

    public bool PrivacyBlocked { get; set; }
}

public class GetCustomerListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public bool? Blocked { get; set; }
}

public class SalesHeaderDto : FullAuditedEntityDto<Guid>
{
    public SalesDocumentType DocumentType { get; set; }
    public string No { get; set; }
    public Guid CustomerId { get; set; }
    public string SellToCustomerNo { get; set; }
    public string SellToCustomerName { get; set; }
    public string BillToCustomerNo { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DocumentStatus Status { get; set; }
    public string CurrencyCode { get; set; }
    public string PaymentTermsCode { get; set; }
    public string LocationCode { get; set; }
    public string ExternalDocumentNo { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalAmountIncludingVat { get; set; }
    public bool Posted { get; set; }
    public string PostedDocumentNo { get; set; }
    public List<SalesLineDto> Lines { get; set; } = new();
}

public class SalesLineDto : EntityDto<Guid>
{
    public Guid SalesHeaderId { get; set; }
    public int LineNo { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public decimal LineAmount { get; set; }
    public decimal LineAmountIncludingVat { get; set; }
    public string VatBusPostingGroup { get; set; }
    public string VatProdPostingGroup { get; set; }
    public ABPmicroservice.Erp.Finance.VatCalculationType VatCalculationType { get; set; }
    public string VatIdentifier { get; set; }
    public decimal VatPercent { get; set; }
    public decimal VatBaseAmount { get; set; }
    public decimal VatAmount { get; set; }
    public string UnitOfMeasureCode { get; set; }
}

public class CreateUpdateSalesHeaderDto
{
    public SalesDocumentType DocumentType { get; set; }
    public string No { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string CurrencyCode { get; set; }
    public string PaymentTermsCode { get; set; }
    public string LocationCode { get; set; }
    public string ExternalDocumentNo { get; set; }
    public List<SalesLineInputDto> Lines { get; set; } = new();
}

public class SalesLineInputDto
{
    public Guid? Id { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public string UnitOfMeasureCode { get; set; }
}

public class GetSalesDocumentListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public SalesDocumentType? DocumentType { get; set; }
    public DocumentStatus? Status { get; set; }
    public Guid? CustomerId { get; set; }
}
