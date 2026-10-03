namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Mirrors Business Central table 17 "G/L Entry" field 57 "Source Type".
/// </summary>
public enum GLEntrySourceType
{
    None = 0,
    Customer = 1,
    Vendor = 2,
    BankAccount = 3,
    FixedAsset = 4,
    Employee = 5,
}
