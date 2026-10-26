namespace ABPmicroservice.Erp.Emailing;

/// <summary>The document type an email template or sent email log relates to.</summary>
public enum EmailDocumentType
{
    SalesQuote = 0,
    SalesOrder = 1,
    SalesInvoice = 2,
    SalesCreditMemo = 3,
    PostedSalesInvoice = 4,
    PostedSalesCreditMemo = 5,
    PurchaseQuote = 6,
    PurchaseOrder = 7,
    PurchaseInvoice = 8,
    PostedPurchaseInvoice = 9,
}

/// <summary>Delivery status of a document email.</summary>
public enum EmailDeliveryStatus
{
    Queued = 0,
    Sent = 1,
    Failed = 2,
}
