namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Mirrors Business Central "Document Type" for the Purchase Header.
/// </summary>
public enum PurchaseDocumentType
{
    Quote = 0,
    Order = 1,
    Invoice = 2,
    CreditMemo = 3,
    BlanketOrder = 4,
    ReturnOrder = 5
}
