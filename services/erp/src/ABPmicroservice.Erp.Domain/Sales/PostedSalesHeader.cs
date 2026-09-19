using ABPmicroservice.Erp.Companies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Posted Sales Invoice Header. Mirrors Business Central table 112 "Sales Invoice Header".
/// Immutable historical posted document.
/// </summary>
public class PostedSalesHeader : CompanyBasicEntity
{
    public string No { get; private set; }
    public string PreAssignedNo { get; private set; }
    public Guid CustomerId { get; private set; }
    public string SellToCustomerNo { get; private set; }
    public string SellToCustomerName { get; private set; }
    public DateTime PostingDate { get; private set; }
    public DateTime DocumentDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public string CurrencyCode { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal TotalAmountIncludingVat { get; private set; }
    public Guid DimensionSetId { get; private set; }
    public Collection<PostedSalesLine> Lines { get; private set; }

    protected PostedSalesHeader()
    {
        Lines = new Collection<PostedSalesLine>();
    }

    public PostedSalesHeader(
        Guid id,
        string no,
        string preAssignedNo,
        Guid customerId,
        string sellToCustomerNo,
        string sellToCustomerName,
        DateTime postingDate,
        DateTime dueDate,
        decimal totalAmount,
        decimal totalAmountIncludingVat,
        Guid dimensionSetId = default
    )
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength);
        PreAssignedNo = Check.NotNullOrWhiteSpace(preAssignedNo, nameof(preAssignedNo), ErpDomainConsts.MaxDocumentNoLength);
        CustomerId = customerId;
        SellToCustomerNo = Check.NotNullOrWhiteSpace(sellToCustomerNo, nameof(sellToCustomerNo), ErpDomainConsts.MaxNoLength);
        SellToCustomerName = Check.Length(sellToCustomerName, nameof(sellToCustomerName), ErpDomainConsts.MaxNameLength);
        PostingDate = postingDate;
        DocumentDate = postingDate;
        DueDate = dueDate;
        TotalAmount = totalAmount;
        TotalAmountIncludingVat = totalAmountIncludingVat;
        DimensionSetId = dimensionSetId;
        Lines = new Collection<PostedSalesLine>();
    }
}

/// <summary>
/// Posted Sales Invoice Line. Mirrors Business Central table 113 "Sales Invoice Line".
/// </summary>
public class PostedSalesLine : Entity<Guid>
{
    public Guid PostedSalesHeaderId { get; private set; }
    public int LineNo { get; private set; }
    public string Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineAmount { get; private set; }
    public Guid DimensionSetId { get; private set; }

    protected PostedSalesLine() { }

    public PostedSalesLine(
        Guid id,
        Guid postedSalesHeaderId,
        int lineNo,
        string type,
        string no,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal lineAmount,
        Guid dimensionSetId = default
    )
        : base(id)
    {
        PostedSalesHeaderId = postedSalesHeaderId;
        LineNo = lineNo;
        Type = type;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineAmount = lineAmount;
        DimensionSetId = dimensionSetId;
    }
}
