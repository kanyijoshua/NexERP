namespace ABPmicroservice.Erp.Workflows;

/// <summary>
///Status.
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

/// <summary>The kind of record an approval request is about.</summary>
public enum ApprovalDocumentKind
{
    SalesDocument = 0,
    PurchaseDocument = 1,
    PaymentVoucher = 2,
    PurchaseRequisition = 3,
    Tender = 4,
    LeaveApplication = 5,
}

/// <summary>
/// The "Approver Limit Type" option of the
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

    /// <summary>One named user, whatever the sender's chain and limits.</summary>
    SpecificApprover = 3,
}

/// <summary>Who receives the approval requests of a workflow.</summary>
public enum WorkflowApproverType
{
    /// <summary>The Approval User Setup chain, as the approver limit type says.</summary>
    Approver = 0,

    /// <summary>
    /// The members of a workflow user group, in member sequence order. Members that share a
    /// sequence number are asked at the same time and must all approve before the next sequence.
    /// </summary>
    WorkflowUserGroup = 1,
}
