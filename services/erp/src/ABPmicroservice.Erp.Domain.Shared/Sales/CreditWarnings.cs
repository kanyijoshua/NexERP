namespace ABPmicroservice.Erp.Sales;

/// <summary>Mirrors Business Central "Credit Warnings" (table 311 field 7).</summary>
public enum CreditWarnings
{
    BothWarnings = 0,
    CreditLimit = 1,
    OverdueBalance = 2,
    NoWarning = 3,
}
