namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Mirrors Business Central "G/L Account Type" option.
/// </summary>
public enum GLAccountType
{
    Posting = 0,
    Heading = 1,
    Total = 2,
    BeginTotal = 3,
    EndTotal = 4
}
