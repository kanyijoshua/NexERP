namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Mirrors Business Central "Document Type" for the Sales Header.
/// </summary>
public enum SalesDocumentType
{
    Quote = 0,
    Order = 1,
    Invoice = 2,
    CreditMemo = 3,
    BlanketOrder = 4,
    ReturnOrder = 5
}
