namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Mirrors Business Central "Document Type" on a posted G/L Entry.
/// </summary>
public enum GLEntryDocumentType
{
    None = 0,
    Payment = 1,
    Invoice = 2,
    CreditMemo = 3,
    FinanceChargeMemo = 4,
    Reminder = 5,
    Refund = 6
}
