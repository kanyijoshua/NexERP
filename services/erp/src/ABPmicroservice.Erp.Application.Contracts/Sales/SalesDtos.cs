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
    public string CurrencyCode { get; set; }
    public bool Blocked { get; set; }
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
    public string CurrencyCode { get; set; }
}

public class GetCustomerListInput : PagedAndSortedResultRequestDto
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

public class GetSalesDocumentListInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
    public SalesDocumentType? DocumentType { get; set; }
    public DocumentStatus? Status { get; set; }
    public Guid? CustomerId { get; set; }
}
