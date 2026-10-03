namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Mirrors Business Central table 15 "G/L Account" field 10 "Debit/Credit".
/// </summary>
public enum GLAccountDebitCredit
{
    Both = 0,
    Debit = 1,
    Credit = 2,
}
