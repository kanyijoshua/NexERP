using System;
using System.Collections.Generic;
using ABPmicroservice.Erp.Documents;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Sales;

/// <summary>How an order is posted: ship, invoice or both, with optional quantities per line.</summary>
public class SalesOrderPostingInput
{
    public bool Ship { get; set; } = true;
    public bool Invoice { get; set; } = true;

    /// <summary>Quantities to ship and invoice; lines left out keep what is on them.</summary>
    public List<SalesLineQuantitiesInput> Lines { get; set; } = new();
}

public class SalesLineQuantitiesInput
{
    public Guid LineId { get; set; }
    public decimal QtyToShip { get; set; }
    public decimal QtyToInvoice { get; set; }
}

public class SalesPostingResultDto
{
    public SalesHeaderDto Document { get; set; }

    /// <summary>The posted invoice or credit memo number; null when nothing was invoiced.</summary>
    public string InvoiceNo { get; set; }

    /// <summary>The posted shipment or return receipt number; null when nothing was shipped.</summary>
    public string ShipmentNo { get; set; }
}

public class MakeOrderInput
{
    /// <summary>The order's posting date; blank is today.</summary>
    public DateTime? PostingDate { get; set; }
}

public class GetShipmentLinesInput
{
    public List<Guid> ShipmentLineIds { get; set; } = new();
}

public class CopyDocumentLinesInput
{
    public Guid SourceDocumentId { get; set; }
}

/// <summary>A posted shipment (or return receipt) line still waiting for an invoice.</summary>
public class OpenShipmentLineDto
{
    public Guid ShipmentLineId { get; set; }
    public string ShipmentNo { get; set; }
    public DateTime PostingDate { get; set; }
    public string OrderNo { get; set; }
    public int LineNo { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal QtyNotInvoiced { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PostedSalesShipmentDto : EntityDto<Guid>
{
    public string No { get; set; }
    public bool Return { get; set; }
    public string OrderNo { get; set; }
    public Guid CustomerId { get; set; }
    public string SellToCustomerNo { get; set; }
    public string SellToCustomerName { get; set; }
    public DateTime PostingDate { get; set; }
    public string LocationCode { get; set; }
    public string ExternalDocumentNo { get; set; }
    public string CurrencyCode { get; set; }
    public string ShipToName { get; set; }
    public string ShipToAddress { get; set; }
    public string ShipToCity { get; set; }
    public string ShipToPostCode { get; set; }
    public string ShipToCountryRegionCode { get; set; }
    public List<PostedSalesShipmentLineDto> Lines { get; set; } = new();
}

public class PostedSalesShipmentLineDto : EntityDto<Guid>
{
    public Guid PostedSalesShipmentId { get; set; }
    public int LineNo { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasureCode { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public string LocationCode { get; set; }
    public Guid OrderLineId { get; set; }
    public int OrderLineNo { get; set; }
    public Guid? ItemLedgerEntryId { get; set; }
    public decimal QuantityInvoiced { get; set; }
    public bool Correction { get; set; }
}

public class PostedSalesInvoiceDto : EntityDto<Guid>
{
    public string No { get; set; }
    public string PreAssignedNo { get; set; }
    public bool CreditMemo { get; set; }
    public string OrderNo { get; set; }
    public Guid CustomerId { get; set; }
    public string SellToCustomerNo { get; set; }
    public string SellToCustomerName { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime DueDate { get; set; }
    public string CurrencyCode { get; set; }
    public string ExternalDocumentNo { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalAmountIncludingVat { get; set; }
    public List<PostedSalesInvoiceLineDto> Lines { get; set; } = new();
}

public class PostedSalesInvoiceLineDto : EntityDto<Guid>
{
    public Guid PostedSalesHeaderId { get; set; }
    public int LineNo { get; set; }
    public string Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasureCode { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public decimal LineAmount { get; set; }
    public decimal VatPercent { get; set; }
    public decimal VatAmount { get; set; }
    public decimal AmountIncludingVat { get; set; }
    public int? OrderLineNo { get; set; }
    public string ShipmentNo { get; set; }
    public int? ShipmentLineNo { get; set; }
}

public class GetPostedSalesDocumentListInput : ErpPagedListInput
{
    public string Filter { get; set; }

    /// <summary>Return receipts (or credit memos) instead of shipments (or invoices); blank lists both.</summary>
    public bool? Return { get; set; }

    public Guid? CustomerId { get; set; }
    public string OrderNo { get; set; }
}
