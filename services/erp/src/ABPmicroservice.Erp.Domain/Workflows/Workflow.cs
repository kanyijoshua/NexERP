using ABPmicroservice.Erp.Companies;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>
/// Approval workflow definition: the event condition is an amount threshold (plus optional
/// condition rules on the trigger step) and the response is "Create an approval request" for
/// the approvers the approver type and limit type select.
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

    public WorkflowApproverType ApproverType { get; private set; }

    /// <summary>The group whose members approve, when the approver type is a workflow user group.</summary>
    public string WorkflowUserGroupCode { get; private set; }

    /// <summary>The one approver, when the limit type is a specific approver.</summary>
    public Guid? SpecificApproverUserId { get; private set; }

    public string SpecificApproverUserName { get; private set; }

    /// <summary>
    /// An open request nobody has acted on for this many days is delegated automatically to the
    /// approver's substitute (or their own approver). Zero means never.
    /// </summary>
    public int DelegateAfterDays { get; private set; }

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

        // A new workflow does nothing until it is enabled.
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
        Category = documentKind switch
        {
            ApprovalDocumentKind.SalesDocument => "SALES",
            ApprovalDocumentKind.PaymentVoucher => "FINANCE",
            ApprovalDocumentKind.PurchaseRequisition or ApprovalDocumentKind.Tender => "PROCUREMENT",
            ApprovalDocumentKind.LeaveApplication => "HR",
            _ => "PURCH",
        };
        MinimumAmount = Check.Range(minimumAmount, nameof(minimumAmount), 0m, decimal.MaxValue);
        ApproverLimitType = approverLimitType;
        DueDays = Check.Range(dueDays, nameof(dueDays), 0, 3650);
    }

    /// <summary>
    /// Who the requests go to. A group needs its code, a specific approver needs the user;
    /// whatever the other choice would need is cleared. Call after <see cref="Update"/>, which sets the limit type.
    /// </summary>
    public void SetApprovers(
        WorkflowApproverType approverType,
        string workflowUserGroupCode,
        Guid? specificApproverUserId,
        string specificApproverUserName
    )
    {
        ApproverType = approverType;

        if (approverType == WorkflowApproverType.WorkflowUserGroup)
        {
            if (workflowUserGroupCode.IsNullOrWhiteSpace())
            {
                throw new BusinessException(ErpErrorCodes.Approvals.WorkflowUserGroupRequired);
            }

            WorkflowUserGroupCode = CodeTableEntity.NormalizeCode(
                Check.Length(workflowUserGroupCode, nameof(workflowUserGroupCode), ErpDomainConsts.MaxCodeLength));
        }
        else
        {
            WorkflowUserGroupCode = null;
        }

        if (approverType == WorkflowApproverType.Approver && ApproverLimitType == ApproverLimitType.SpecificApprover)
        {
            if (!specificApproverUserId.HasValue)
            {
                throw new BusinessException(ErpErrorCodes.Approvals.SpecificApproverRequired);
            }

            SpecificApproverUserId = specificApproverUserId;
            SpecificApproverUserName = Check.Length(specificApproverUserName, nameof(specificApproverUserName), ErpDomainConsts.MaxUserNameLength);
        }
        else
        {
            SpecificApproverUserId = null;
            SpecificApproverUserName = null;
        }
    }

    public void SetDelegateAfterDays(int delegateAfterDays)
    {
        DelegateAfterDays = Check.Range(delegateAfterDays, nameof(delegateAfterDays), 0, 365);
    }

    /// <summary>A disabled duplicate under a new code, steps included.</summary>
    public Workflow CopyAs(Guid id, string code, string description, Func<Guid> newId)
    {
        var copy = new Workflow(id, code, description.IsNullOrWhiteSpace() ? Description : description,
            DocumentKind, MinimumAmount, ApproverLimitType, DueDays);
        copy.SetApprovers(ApproverType, WorkflowUserGroupCode, SpecificApproverUserId, SpecificApproverUserName);
        copy.SetDelegateAfterDays(DelegateAfterDays);
        copy.SetSteps(Steps.OrderBy(s => s.SequenceNo).Select(s => (s.EventName, s.ConditionRule, s.ResponseAction)), newId);
        return copy;
    }

    public void Enable() => Enabled = true;

    public void Disable() => Enabled = false;

    /// <summary>
    /// Rewrites the readable step list from the settings above.
    /// The steps document the behaviour; ApprovalsManager carries it out.
    /// </summary>
    public void RebuildSteps(Func<Guid> newId)
    {
        Steps.Clear();

        var condition = MinimumAmount > 0m ? $"Amount >= {MinimumAmount:0.##}" : "Always";

        AddStep(newId, RequestedEventName(DocumentKind), condition,
            $"Set the document status to Pending Approval. Create an approval request ({DescribeApprovers()}). Send the first request.");
        AddStep(newId, "An approval request is approved.", "Pending approvals: more than 0", "Send the next approval request.");
        AddStep(newId, "An approval request is approved.", "Pending approvals: 0", "Release the document.");
        AddStep(newId, "An approval request is rejected.", "Always", "Reject all approval requests. Reopen the document.");
        AddStep(newId, "An approval request is canceled.", "Always", "Cancel all approval requests. Reopen the document.");
        AddStep(newId, "An approval request is delegated.", "Always", "Send the request to the substitute, or to the approver's approver.");

        if (DelegateAfterDays > 0)
        {
            AddStep(newId, "An approval request is not acted on.", $"Open for {DelegateAfterDays} day(s)",
                "Delegate the request to the substitute, or to the approver's approver.");
        }
    }

    /// <summary>The trigger event of a document kind, as the first step shows it.</summary>
    public static string RequestedEventName(ApprovalDocumentKind kind)
    {
        var document = kind switch
        {
            ApprovalDocumentKind.SalesDocument => "sales document",
            ApprovalDocumentKind.PaymentVoucher => "payment voucher",
            ApprovalDocumentKind.PurchaseRequisition => "purchase requisition",
            ApprovalDocumentKind.Tender => "tender",
            ApprovalDocumentKind.LeaveApplication => "leave application",
            _ => "purchase document",
        };

        return $"Approval of a {document} is requested.";
    }

    private string DescribeApprovers()
    {
        if (ApproverType == WorkflowApproverType.WorkflowUserGroup)
        {
            return $"workflow user group {WorkflowUserGroupCode}";
        }

        return ApproverLimitType == ApproverLimitType.SpecificApprover
            ? $"specific approver {SpecificApproverUserName}"
            : ApproverLimitType.ToString();
    }

    public void SetSteps(System.Collections.Generic.IEnumerable<(string EventName, string ConditionRule, string ResponseAction)> steps, Func<Guid> newId)
    {
        Steps.Clear();
        foreach (var step in steps)
        {
            AddStep(newId, step.EventName, step.ConditionRule, step.ResponseAction);
        }
    }

    private void AddStep(Func<Guid> newId, string eventName, string condition, string response)
    {
        Steps.Add(new WorkflowStep(newId(), Id, Steps.Count + 1, eventName, condition, response));
    }
}

/// <summary>
/// Workflow Step.
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
        ConditionRule = string.IsNullOrWhiteSpace(conditionRule) ? "Always" : conditionRule;
        ResponseAction = Check.NotNullOrWhiteSpace(responseAction, nameof(responseAction));
    }
}

/// <summary>
/// Approval Entry: one row per approver
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

    /// <summary>When the entry was last opened or delegated: what the delegate-after timeout counts from.</summary>
    public DateTime WaitingSince => LastStatusChangeTime ?? CreationTime;

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
/// Approval User Setup. Mirrors the approval fields of "User Setup":
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

    /// <summary>May approve, reject, delegate and cancel any request.</summary>
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

    /// <summary>
    /// Whether this user's limit covers a document of the given kind and amount. A payment voucher
    /// is money going out, so it is held to the purchase limit.
    /// </summary>
    public bool CanApprove(ApprovalDocumentKind kind, decimal amount)
    {
        return kind == ApprovalDocumentKind.SalesDocument
            ? UnlimitedSalesApproval || amount <= SalesAmountApprovalLimit
            : UnlimitedPurchaseApproval || amount <= PurchaseAmountApprovalLimit;
    }
}
