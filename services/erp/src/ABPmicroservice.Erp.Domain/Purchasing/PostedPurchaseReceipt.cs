using System;
using System.Collections.ObjectModel;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Posted Purchase Receipt: the goods that came in from the vendor on one posting of an order. With
/// <see cref="Return"/> set it is a posted return shipment: goods sent back on a return order.
/// </summary>
public class PostedPurchaseReceipt : CompanyBasicEntity
{
    public string No { get; private set; }

    /// <summary>A return shipment rather than a receipt.</summary>
    public bool Return { get; private set; }

    public string OrderNo { get; private set; }
    public Guid VendorId { get; private set; }
    public string BuyFromVendorNo { get; private set; }
    public string BuyFromVendorName { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string LocationCode { get; private set; }
    public string VendorShipmentNo { get; private set; }
    public string CurrencyCode { get; private set; }

    public Collection<PostedPurchaseReceiptLine> Lines { get; private set; }

    protected PostedPurchaseReceipt()
    {
        Lines = new Collection<PostedPurchaseReceiptLine>();
    }

    public PostedPurchaseReceipt(Guid id, string no, bool isReturn, PurchaseHeader order)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength);
        Return = isReturn;
        OrderNo = order.No;
        VendorId = order.VendorId;
        BuyFromVendorNo = order.BuyFromVendorNo;
        BuyFromVendorName = order.BuyFromVendorName;
        PostingDate = order.PostingDate;
        LocationCode = order.LocationCode;
        VendorShipmentNo = order.VendorShipmentNo;
        CurrencyCode = order.CurrencyCode;
        Lines = new Collection<PostedPurchaseReceiptLine>();
    }
}

/// <summary>One received (or returned) line, with how much of it has been invoiced since.</summary>
public class PostedPurchaseReceiptLine : Entity<Guid>
{
    public Guid PostedPurchaseReceiptId { get; private set; }
    public int LineNo { get; private set; }
    public DocumentLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public string UnitOfMeasureCode { get; private set; }
    public decimal DirectUnitCost { get; private set; }
    public decimal LineDiscountPercent { get; private set; }
    public string LocationCode { get; private set; }

    /// <summary>The order line it was received on.</summary>
    public Guid OrderLineId { get; private set; }
    public int OrderLineNo { get; private set; }

    /// <summary>The stock movement it posted; none for a G/L account line.</summary>
    public Guid? ItemLedgerEntryId { get; private set; }

    public decimal QuantityInvoiced { get; private set; }

    /// <summary>Set once the line has been undone; the undo posts the stock back.</summary>
    public bool Correction { get; private set; }

    public decimal QtyNotInvoiced => Correction ? 0m : Quantity - QuantityInvoiced;

    protected PostedPurchaseReceiptLine() { }

    public string VariantCode { get; private set; }
    public decimal QtyPerUnitOfMeasure { get; private set; } = 1m;

    public PostedPurchaseReceiptLine(Guid id, Guid postedPurchaseReceiptId, PurchaseLine orderLine, decimal quantity, string locationCode, Guid? itemLedgerEntryId)
        : base(id)
    {
        VariantCode = orderLine.VariantCode;
        QtyPerUnitOfMeasure = orderLine.QtyPerUnitOfMeasure;
        PostedPurchaseReceiptId = postedPurchaseReceiptId;
        LineNo = orderLine.LineNo;
        Type = orderLine.Type;
        No = orderLine.No;
        Description = orderLine.Description;
        Quantity = quantity;
        UnitOfMeasureCode = orderLine.UnitOfMeasureCode;
        DirectUnitCost = orderLine.DirectUnitCost;
        LineDiscountPercent = orderLine.LineDiscountPercent;
        LocationCode = locationCode;
        OrderLineId = orderLine.Id;
        OrderLineNo = orderLine.LineNo;
        ItemLedgerEntryId = itemLedgerEntryId;
    }

    internal void RecordInvoiced(decimal quantity)
    {
        QuantityInvoiced += quantity;
    }

    internal void MarkUndone()
    {
        Correction = true;
    }
}
