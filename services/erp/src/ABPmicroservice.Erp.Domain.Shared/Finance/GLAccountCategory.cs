namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Mirrors Business Central "G/L Account Category" option.
/// </summary>
public enum GLAccountCategory
{
    Assets = 0,
    Liabilities = 1,
    Equity = 2,
    Income = 3,
    CostOfGoodsSold = 4,
    Expense = 5
}
