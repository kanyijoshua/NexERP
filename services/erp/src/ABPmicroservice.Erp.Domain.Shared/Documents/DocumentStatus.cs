namespace ABPmicroservice.Erp.Documents;

/// <summary>
/// Mirrors Business Central "Status" on sales/purchase documents.
/// </summary>
public enum DocumentStatus
{
    Open = 0,
    Released = 1,
    PendingApproval = 2,
    PendingPrepayment = 3,
    Posted = 4,
    Cancelled = 5
}
