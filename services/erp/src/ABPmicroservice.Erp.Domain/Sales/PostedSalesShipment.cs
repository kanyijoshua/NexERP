using System;
using System.Collections.ObjectModel;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Posted Sales Shipment: the goods that left for the customer on one posting of an order. With
/// <see cref="Return"/> set it is a posted return receipt: goods the customer sent back on a return order.
/// </summary>
public class PostedSalesShipment : CompanyBasicEntity
{
    public string No { get; private set; }

    /// <summary>A return receipt rather than a shipment.</summary>
    public bool Return { get; private set; }

    public string OrderNo { get; private set; }
    public Guid CustomerId { get; private set; }
    public string SellToCustomerNo { get; private set; }
    public string SellToCustomerName { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string LocationCode { get; private set; }
    public string ExternalDocumentNo { get; private set; }
    public string CurrencyCode { get; private set; }

    public string ShipToName { get; private set; }
    public string ShipToAddress { get; private set; }
    public string ShipToCity { get; private set; }
    public string ShipToPostCode { get; private set; }
    public string ShipToCountryRegionCode { get; private set; }

    public Collection<PostedSalesShipmentLine> Lines { get; private set; }

    protected PostedSalesShipment()
    {
        Lines = new Collection<PostedSalesShipmentLine>();
    }

    public PostedSalesShipment(Guid id, string no, bool isReturn, SalesHeader order)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength);
        Return = isReturn;
        OrderNo = order.No;
        CustomerId = order.CustomerId;
        SellToCustomerNo = order.SellToCustomerNo;
        SellToCustomerName = order.SellToCustomerName;
        PostingDate = order.PostingDate;
        LocationCode = order.LocationCode;
        ExternalDocumentNo = order.ExternalDocumentNo;
        CurrencyCode = order.CurrencyCode;
        ShipToName = order.ShipToName;
        ShipToAddress = order.ShipToAddress;
        ShipToCity = order.ShipToCity;
        ShipToPostCode = order.ShipToPostCode;
        ShipToCountryRegionCode = order.ShipToCountryRegionCode;
        Lines = new Collection<PostedSalesShipmentLine>();
    }
}

/// <summary>One shipped (or returned) line, with how much of it has been invoiced since.</summary>
public class PostedSalesShipmentLine : Entity<Guid>
{
    public Guid PostedSalesShipmentId { get; private set; }
    public int LineNo { get; private set; }
    public DocumentLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public string UnitOfMeasureCode { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineDiscountPercent { get; private set; }
    public string LocationCode { get; private set; }

    /// <summary>The order line it was shipped from.</summary>
    public Guid OrderLineId { get; private set; }
    public int OrderLineNo { get; private set; }

    /// <summary>The stock movement it posted; none for a G/L account line.</summary>
    public Guid? ItemLedgerEntryId { get; private set; }

    public decimal QuantityInvoiced { get; private set; }

    /// <summary>Set once the line has been undone; the undo posts the stock back.</summary>
    public bool Correction { get; private set; }

    public decimal QtyNotInvoiced => Correction ? 0m : Quantity - QuantityInvoiced;

    protected PostedSalesShipmentLine() { }

    public string VariantCode { get; private set; }
    public decimal QtyPerUnitOfMeasure { get; private set; } = 1m;

    public PostedSalesShipmentLine(Guid id, Guid postedSalesShipmentId, SalesLine orderLine, decimal quantity, string locationCode, Guid? itemLedgerEntryId)
        : base(id)
    {
        VariantCode = orderLine.VariantCode;
        QtyPerUnitOfMeasure = orderLine.QtyPerUnitOfMeasure;
        PostedSalesShipmentId = postedSalesShipmentId;
        LineNo = orderLine.LineNo;
        Type = orderLine.Type;
        No = orderLine.No;
        Description = orderLine.Description;
        Quantity = quantity;
        UnitOfMeasureCode = orderLine.UnitOfMeasureCode;
        UnitPrice = orderLine.UnitPrice;
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
