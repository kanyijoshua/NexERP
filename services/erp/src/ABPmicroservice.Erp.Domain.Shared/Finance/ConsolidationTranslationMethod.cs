namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Mirrors Business Central table 15 "G/L Account" field 39 "Consol. Translation Method".
/// </summary>
public enum ConsolidationTranslationMethod
{
    Average = 0,
    Closing = 1,
    Historical = 2,
    Composite = 3,
}
