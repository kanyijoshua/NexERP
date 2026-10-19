using System;
using System.Collections.Generic;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>How an order is posted: receive, invoice or both, with optional quantities per line.</summary>
public class PurchaseOrderPostingInput
{
    public bool Receive { get; set; } = true;
    public bool Invoice { get; set; } = true;

    /// <summary>Quantities to receive and invoice; lines left out keep what is on them.</summary>
    public List<PurchaseLineQuantitiesInput> Lines { get; set; } = new();
}

public class PurchaseLineQuantitiesInput
{
    public Guid LineId { get; set; }
    public decimal QtyToReceive { get; set; }
    public decimal QtyToInvoice { get; set; }
}

public class PurchasePostingResultDto
{
    public PurchaseHeaderDto Document { get; set; }

    /// <summary>The posted invoice or credit memo number; null when nothing was invoiced.</summary>
    public string InvoiceNo { get; set; }

    /// <summary>The posted receipt or return shipment number; null when nothing was received.</summary>
    public string ReceiptNo { get; set; }
}

public class GetReceiptLinesInput
{
    public List<Guid> ReceiptLineIds { get; set; } = new();
}

/// <summary>A posted receipt (or return shipment) line still waiting for an invoice.</summary>
public class OpenReceiptLineDto
{
    public Guid ReceiptLineId { get; set; }
    public string ReceiptNo { get; set; }
    public DateTime PostingDate { get; set; }
    public string OrderNo { get; set; }
    public int LineNo { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal QtyNotInvoiced { get; set; }
    public decimal DirectUnitCost { get; set; }
}

public class PostedPurchaseReceiptDto : EntityDto<Guid>
{
    public string No { get; set; }
    public bool Return { get; set; }
    public string OrderNo { get; set; }
    public Guid VendorId { get; set; }
    public string BuyFromVendorNo { get; set; }
    public string BuyFromVendorName { get; set; }
    public DateTime PostingDate { get; set; }
    public string LocationCode { get; set; }
    public string VendorShipmentNo { get; set; }
    public string CurrencyCode { get; set; }
    public List<PostedPurchaseReceiptLineDto> Lines { get; set; } = new();
}

public class PostedPurchaseReceiptLineDto : EntityDto<Guid>
{
    public Guid PostedPurchaseReceiptId { get; set; }
    public int LineNo { get; set; }
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasureCode { get; set; }
    public decimal DirectUnitCost { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public string LocationCode { get; set; }
    public Guid OrderLineId { get; set; }
    public int OrderLineNo { get; set; }
    public Guid? ItemLedgerEntryId { get; set; }
    public decimal QuantityInvoiced { get; set; }
    public bool Correction { get; set; }
}

public class PostedPurchaseInvoiceDto : EntityDto<Guid>
{
    public string No { get; set; }
    public string PreAssignedNo { get; set; }
    public bool CreditMemo { get; set; }
    public string OrderNo { get; set; }
    public Guid VendorId { get; set; }
    public string BuyFromVendorNo { get; set; }
    public string BuyFromVendorName { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime DueDate { get; set; }
    public string CurrencyCode { get; set; }
    public string VendorInvoiceNo { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalAmountIncludingVat { get; set; }
    public List<PostedPurchaseInvoiceLineDto> Lines { get; set; } = new();
}

public class PostedPurchaseInvoiceLineDto : EntityDto<Guid>
{
    public Guid PostedPurchaseHeaderId { get; set; }
    public int LineNo { get; set; }
    public string Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasureCode { get; set; }
    public decimal DirectUnitCost { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public decimal LineAmount { get; set; }
    public decimal VatPercent { get; set; }
    public decimal VatAmount { get; set; }
    public decimal AmountIncludingVat { get; set; }
    public int? OrderLineNo { get; set; }
    public string ReceiptNo { get; set; }
    public int? ReceiptLineNo { get; set; }
}

public class GetPostedPurchaseDocumentListInput : ErpPagedListInput
{
    public string Filter { get; set; }

    /// <summary>Return shipments (or credit memos) instead of receipts (or invoices); blank lists both.</summary>
    public bool? Return { get; set; }

    public Guid? VendorId { get; set; }
    public string OrderNo { get; set; }
}
