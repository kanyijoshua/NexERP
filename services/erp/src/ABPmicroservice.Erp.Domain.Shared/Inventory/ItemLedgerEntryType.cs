namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Mirrors Business Central "Entry Type" on the Item Ledger Entry.
/// </summary>
public enum ItemLedgerEntryType
{
    Purchase = 0,
    Sale = 1,
    PositiveAdjmt = 2,
    NegativeAdjmt = 3,
    Transfer = 4,
    Consumption = 5,
    Output = 6
}
