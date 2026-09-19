using ABPmicroservice.Erp.Companies;
using System;
using System.Collections.ObjectModel;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>
/// Approval workflow definition. Mirrors Business Central table 1501 "Workflow", narrowed to the
/// approval templates ("Sales Invoice Approval Workflow" and its siblings): the event condition
/// is an amount threshold and the response is "Create an approval request" with an approver limit type.
/// </summary>
public class Workflow : CompanyAggregateRoot
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public bool Enabled { get; private set; }

    /// <summary>The kind of document this workflow guards.</summary>
    public ApprovalDocumentKind DocumentKind { get; private set; }

    /// <summary>Event condition: only documents of at least this amount need approval. Zero means all.</summary>
    public decimal MinimumAmount { get; private set; }

    public ApproverLimitType ApproverLimitType { get; private set; }

    /// <summary>Days an approver has to respond; sets the due date of each request. Zero means none.</summary>
    public int DueDays { get; private set; }

    public Collection<WorkflowStep> Steps { get; private set; }

    protected Workflow()
    {
        Steps = new Collection<WorkflowStep>();
    }

    public Workflow(
        Guid id,
        string code,
        string description,
        ApprovalDocumentKind documentKind,
        decimal minimumAmount = 0m,
        ApproverLimitType approverLimitType = ApproverLimitType.ApproverChain,
        int dueDays = 0
    )
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxWorkflowCodeLength);
        Steps = new Collection<WorkflowStep>();

        // As in Business Central, a new workflow does nothing until it is enabled.
        Enabled = false;
        Update(description, documentKind, minimumAmount, approverLimitType, dueDays);
    }

    public void Update(
        string description,
        ApprovalDocumentKind documentKind,
        decimal minimumAmount,
        ApproverLimitType approverLimitType,
        int dueDays
    )
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        DocumentKind = documentKind;
        Category = documentKind == ApprovalDocumentKind.SalesDocument ? "SALES" : "PURCH";
        MinimumAmount = Check.Range(minimumAmount, nameof(minimumAmount), 0m, decimal.MaxValue);
        ApproverLimitType = approverLimitType;
        DueDays = Check.Range(dueDays, nameof(dueDays), 0, 3650);
    }

    public void Enable() => Enabled = true;

    public void Disable() => Enabled = false;

    /// <summary>
    /// Rewrites the readable step list (BC's workflow step grid) from the settings above.
    /// The steps document the behaviour; ApprovalsManager carries it out.
    /// </summary>
    public void RebuildSteps(Func<Guid> newId)
    {
        Steps.Clear();

        var document = DocumentKind == ApprovalDocumentKind.SalesDocument ? "sales document" : "purchase document";
        var condition = MinimumAmount > 0m ? $"Amount >= {MinimumAmount:0.##}" : "Always";

        AddStep(newId, $"Approval of a {document} is requested.", condition,
            $"Set the document status to Pending Approval. Create an approval request ({ApproverLimitType}). Send the first request.");
        AddStep(newId, "An approval request is approved.", "Pending approvals: more than 0", "Send the next approval request.");
        AddStep(newId, "An approval request is approved.", "Pending approvals: 0", "Release the document.");
        AddStep(newId, "An approval request is rejected.", "Always", "Reject all approval requests. Reopen the document.");
        AddStep(newId, "An approval request is canceled.", "Always", "Cancel all approval requests. Reopen the document.");
        AddStep(newId, "An approval request is delegated.", "Always", "Send the request to the substitute, or to the approver's approver.");
    }

    private void AddStep(Func<Guid> newId, string eventName, string condition, string response)
    {
        Steps.Add(new WorkflowStep(newId(), Id, Steps.Count + 1, eventName, condition, response));
    }
}

/// <summary>
/// Workflow Step. Mirrors Business Central table 1502 "Workflow Step".
/// </summary>
public class WorkflowStep : FullAuditedEntity<Guid>
{
    public Guid WorkflowId { get; private set; }
    public int SequenceNo { get; private set; }
    public string EventName { get; private set; }
    public string ConditionRule { get; private set; }
    public string ResponseAction { get; private set; }

    protected WorkflowStep() { }

    public WorkflowStep(Guid id, Guid workflowId, int sequenceNo, string eventName, string conditionRule, string responseAction)
        : base(id)
    {
        WorkflowId = workflowId;
        SequenceNo = sequenceNo;
        EventName = Check.NotNullOrWhiteSpace(eventName, nameof(eventName));
        ConditionRule = conditionRule;
        ResponseAction = Check.NotNullOrWhiteSpace(responseAction, nameof(responseAction));
    }
}

/// <summary>
/// Approval Entry. Mirrors Business Central table 454 "Approval Entry": one row per approver
/// of a request, worked through in sequence.
/// </summary>
public class ApprovalEntry : CompanyEntity
{
    public ApprovalDocumentKind DocumentKind { get; private set; }
    public Guid DocumentId { get; private set; }
    public string DocumentNo { get; private set; }
    public string WorkflowCode { get; private set; }
    public int SequenceNo { get; private set; }
    public Guid SenderId { get; private set; }
    public string SenderUserName { get; private set; }
    public Guid ApproverId { get; private set; }
    public string ApproverUserName { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime? DueDate { get; private set; }
    public string Comment { get; private set; }
    public DateTime? LastStatusChangeTime { get; private set; }

    protected ApprovalEntry() { }

    public ApprovalEntry(
        Guid id,
        ApprovalDocumentKind documentKind,
        Guid documentId,
        string documentNo,
        string workflowCode,
        int sequenceNo,
        Guid senderId,
        string senderUserName,
        Guid approverId,
        string approverUserName,
        decimal amount,
        DateTime? dueDate
    )
        : base(id)
    {
        DocumentKind = documentKind;
        DocumentId = documentId;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        WorkflowCode = workflowCode;
        SequenceNo = sequenceNo;
        SenderId = senderId;
        SenderUserName = senderUserName;
        ApproverId = approverId;
        ApproverUserName = approverUserName;
        Amount = amount;
        DueDate = dueDate;
        Status = ApprovalStatus.Created;
    }

    public bool IsPending => Status is ApprovalStatus.Created or ApprovalStatus.Open;

    internal void Open(DateTime now) => SetStatus(ApprovalStatus.Open, now);

    internal void Approve(DateTime now, string comment = null) => SetStatus(ApprovalStatus.Approved, now, comment);

    internal void Reject(DateTime now, string comment = null) => SetStatus(ApprovalStatus.Rejected, now, comment);

    internal void Cancel(DateTime now) => SetStatus(ApprovalStatus.Canceled, now);

    internal void DelegateTo(Guid approverId, string approverUserName, DateTime now)
    {
        ApproverId = approverId;
        ApproverUserName = approverUserName;
        LastStatusChangeTime = now;
    }

    private void SetStatus(ApprovalStatus status, DateTime now, string comment = null)
    {
        Status = status;
        LastStatusChangeTime = now;
        if (!comment.IsNullOrWhiteSpace())
        {
            Comment = Check.Length(comment.Trim(), nameof(comment), ErpDomainConsts.MaxCommentLength);
        }
    }
}

/// <summary>
/// Approval User Setup. Mirrors the approval fields of Business Central table 91 "User Setup":
/// who approves a user's requests, up to what amount the user may approve, and who stands in.
/// </summary>
public class ApprovalUserSetup : CompanyEntity
{
    public Guid UserId { get; private set; }
    public string UserName { get; private set; }
    public Guid? ApproverUserId { get; private set; }
    public Guid? SubstituteUserId { get; private set; }
    public decimal SalesAmountApprovalLimit { get; private set; }
    public bool UnlimitedSalesApproval { get; private set; }
    public decimal PurchaseAmountApprovalLimit { get; private set; }
    public bool UnlimitedPurchaseApproval { get; private set; }

    /// <summary>May approve, reject, delegate and cancel any request (BC: Approval Administrator).</summary>
    public bool IsApprovalAdministrator { get; private set; }

    protected ApprovalUserSetup() { }

    public ApprovalUserSetup(Guid id, Guid userId, string userName)
        : base(id)
    {
        UserId = userId;
        UserName = Check.NotNullOrWhiteSpace(userName, nameof(userName), ErpDomainConsts.MaxUserNameLength);
    }

    public void Update(
        string userName,
        Guid? approverUserId,
        Guid? substituteUserId,
        decimal salesAmountApprovalLimit,
        bool unlimitedSalesApproval,
        decimal purchaseAmountApprovalLimit,
        bool unlimitedPurchaseApproval,
        bool isApprovalAdministrator
    )
    {
        UserName = Check.NotNullOrWhiteSpace(userName, nameof(userName), ErpDomainConsts.MaxUserNameLength);

        // A user who approves their own requests would make the chain pointless.
        ApproverUserId = approverUserId == UserId ? null : approverUserId;
        SubstituteUserId = substituteUserId == UserId ? null : substituteUserId;

        SalesAmountApprovalLimit = Check.Range(salesAmountApprovalLimit, nameof(salesAmountApprovalLimit), 0m, decimal.MaxValue);
        UnlimitedSalesApproval = unlimitedSalesApproval;
        PurchaseAmountApprovalLimit = Check.Range(purchaseAmountApprovalLimit, nameof(purchaseAmountApprovalLimit), 0m, decimal.MaxValue);
        UnlimitedPurchaseApproval = unlimitedPurchaseApproval;
        IsApprovalAdministrator = isApprovalAdministrator;
    }

    /// <summary>Whether this user's limit covers a document of the given kind and amount.</summary>
    public bool CanApprove(ApprovalDocumentKind kind, decimal amount)
    {
        return kind == ApprovalDocumentKind.SalesDocument
            ? UnlimitedSalesApproval || amount <= SalesAmountApprovalLimit
            : UnlimitedPurchaseApproval || amount <= PurchaseAmountApprovalLimit;
    }
}
