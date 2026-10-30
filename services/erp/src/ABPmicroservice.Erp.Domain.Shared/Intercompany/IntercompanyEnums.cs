namespace ABPmicroservice.Erp.Intercompany;

/// <summary>The posted document an intercompany transaction comes from.</summary>
public enum ICSourceType
{
    SalesInvoice = 0,
    SalesCreditMemo = 1,
    PurchaseInvoice = 2,
    PurchaseCreditMemo = 3,
}

/// <summary>The document accepting an inbox transaction makes in the receiving company.</summary>
public enum ICDocumentType
{
    PurchaseInvoice = 0,
    PurchaseCreditMemo = 1,
    SalesInvoice = 2,
    SalesCreditMemo = 3,
}

public enum ICOutboxStatus
{
    New = 0,
    Sent = 1,
    Cancelled = 2,
}

public enum ICInboxStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
}

public enum ICLineType
{
    Item = 0,
    GLAccount = 1,
}
