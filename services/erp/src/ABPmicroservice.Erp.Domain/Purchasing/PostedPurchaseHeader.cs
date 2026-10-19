using ABPmicroservice.Erp.Companies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Posted Purchase Invoice Header.
/// Immutable historical posted document.
/// </summary>
public class PostedPurchaseHeader : CompanyBasicEntity
{
    public string No { get; private set; }
    public string PreAssignedNo { get; private set; }
    public Guid VendorId { get; private set; }
    public string BuyFromVendorNo { get; private set; }
    public string BuyFromVendorName { get; private set; }
    public DateTime PostingDate { get; private set; }
    public DateTime DocumentDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public string CurrencyCode { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal TotalAmountIncludingVat { get; private set; }
    public Guid DimensionSetId { get; private set; }

    /// <summary>A posted credit memo rather than an invoice.</summary>
    public bool CreditMemo { get; private set; }

    /// <summary>The order (or return order) this was invoiced from; blank for a stand-alone invoice.</summary>
    public string OrderNo { get; private set; }

    public string VendorInvoiceNo { get; private set; }
    public Collection<PostedPurchaseLine> Lines { get; private set; }

    protected PostedPurchaseHeader()
    {
        Lines = new Collection<PostedPurchaseLine>();
    }

    public PostedPurchaseHeader(
        Guid id,
        string no,
        string preAssignedNo,
        Guid vendorId,
        string buyFromVendorNo,
        string buyFromVendorName,
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
        VendorId = vendorId;
        BuyFromVendorNo = Check.NotNullOrWhiteSpace(buyFromVendorNo, nameof(buyFromVendorNo), ErpDomainConsts.MaxNoLength);
        BuyFromVendorName = Check.Length(buyFromVendorName, nameof(buyFromVendorName), ErpDomainConsts.MaxNameLength);
        PostingDate = postingDate;
        DocumentDate = postingDate;
        DueDate = dueDate;
        TotalAmount = totalAmount;
        TotalAmountIncludingVat = totalAmountIncludingVat;
        DimensionSetId = dimensionSetId;
        Lines = new Collection<PostedPurchaseLine>();
    }

    public void SetDetails(bool creditMemo, string orderNo, string currencyCode, string vendorInvoiceNo)
    {
        CreditMemo = creditMemo;
        OrderNo = Check.Length(orderNo, nameof(orderNo), ErpDomainConsts.MaxDocumentNoLength);
        CurrencyCode = Check.Length(currencyCode, nameof(currencyCode), ErpDomainConsts.MaxCurrencyCodeLength);
        VendorInvoiceNo = Check.Length(vendorInvoiceNo, nameof(vendorInvoiceNo), ErpDomainConsts.MaxExternalDocumentNoLength);
    }
}

/// <summary>
/// Posted Purchase Invoice Line.
/// </summary>
public class PostedPurchaseLine : Entity<Guid>
{
    public Guid PostedPurchaseHeaderId { get; private set; }
    public int LineNo { get; private set; }
    public string Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal DirectUnitCost { get; private set; }
    public decimal LineAmount { get; private set; }
    public Guid DimensionSetId { get; private set; }
    public decimal LineDiscountPercent { get; private set; }
    public string UnitOfMeasureCode { get; private set; }
    public decimal VatPercent { get; private set; }
    public decimal VatAmount { get; private set; }
    public decimal AmountIncludingVat { get; private set; }

    /// <summary>The order line, and the receipt line where the goods came in, that this line invoices.</summary>
    public int? OrderLineNo { get; private set; }
    public string ReceiptNo { get; private set; }
    public int? ReceiptLineNo { get; private set; }

    protected PostedPurchaseLine() { }

    public PostedPurchaseLine(
        Guid id,
        Guid postedPurchaseHeaderId,
        int lineNo,
        string type,
        string no,
        string description,
        decimal quantity,
        decimal directUnitCost,
        decimal lineAmount,
        Guid dimensionSetId = default
    )
        : base(id)
    {
        PostedPurchaseHeaderId = postedPurchaseHeaderId;
        LineNo = lineNo;
        Type = type;
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Quantity = quantity;
        DirectUnitCost = directUnitCost;
        LineAmount = lineAmount;
        DimensionSetId = dimensionSetId;
    }

    public void SetDetails(decimal lineDiscountPercent, string unitOfMeasureCode, decimal vatPercent, decimal vatAmount, decimal amountIncludingVat)
    {
        LineDiscountPercent = lineDiscountPercent;
        UnitOfMeasureCode = Check.Length(unitOfMeasureCode, nameof(unitOfMeasureCode), ErpDomainConsts.MaxUnitOfMeasureCodeLength);
        VatPercent = vatPercent;
        VatAmount = vatAmount;
        AmountIncludingVat = amountIncludingVat;
    }

    public void SetSource(int? orderLineNo, string receiptNo, int? receiptLineNo)
    {
        OrderLineNo = orderLineNo;
        ReceiptNo = Check.Length(receiptNo, nameof(receiptNo), ErpDomainConsts.MaxDocumentNoLength);
        ReceiptLineNo = receiptLineNo;
    }
}
