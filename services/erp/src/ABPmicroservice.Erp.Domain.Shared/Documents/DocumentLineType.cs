namespace ABPmicroservice.Erp.Documents;

/// <summary>
/// Mirrors Business Central "Type" option on sales/purchase lines.
/// </summary>
public enum DocumentLineType
{
    None = 0,
    GLAccount = 1,
    Item = 2,
    Resource = 3,
    FixedAsset = 4,
    ChargeItem = 5
}
