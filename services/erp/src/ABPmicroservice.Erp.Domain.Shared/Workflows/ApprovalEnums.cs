namespace ABPmicroservice.Erp.Workflows;

/// <summary>
/// Mirrors Business Central "Approval Entry".Status.
/// Only one entry of a request is Open at a time; the ones behind it wait as Created.
/// </summary>
public enum ApprovalStatus
{
    Created = 0,
    Open = 1,
    Canceled = 2,
    Rejected = 3,
    Approved = 4,
}

/// <summary>The kind of record an approval request is about (BC: "Table ID" on the approval entry).</summary>
public enum ApprovalDocumentKind
{
    SalesDocument = 0,
    PurchaseDocument = 1,
}

/// <summary>
/// Mirrors Business Central's "Approver Limit Type" option of the
/// "Create an approval request" workflow response.
/// </summary>
public enum ApproverLimitType
{
    /// <summary>Every approver up the chain until one has a sufficient amount limit.</summary>
    ApproverChain = 0,

    /// <summary>Only the sender's own approver, whatever their limit.</summary>
    DirectApprover = 1,

    /// <summary>Only the first approver up the chain with a sufficient amount limit.</summary>
    FirstQualifiedApprover = 2,
}
