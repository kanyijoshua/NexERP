using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>
/// Workflow definition. Mirrors Business Central table 1501 "Workflow".
/// </summary>
public class Workflow : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public bool Enabled { get; private set; }
    public Collection<WorkflowStep> Steps { get; private set; }

    protected Workflow()
    {
        Steps = new Collection<WorkflowStep>();
    }

    public Workflow(Guid id, string code, string description, string category = "SALES")
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxWorkflowCodeLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Category = category;
        Enabled = true;
        Steps = new Collection<WorkflowStep>();
    }

    public void Enable() => Enabled = true;
    public void Disable() => Enabled = false;
}

/// <summary>
/// Workflow Step. Mirrors Business Central table 1502 "Workflow Step".
/// </summary>
public class WorkflowStep : FullAuditedEntity<Guid>
{
    public Guid WorkflowId { get; private set; }
    public int SequenceNo { get; private set; }
    public string EventName { get; private set; } // e.g. "SalesHeader_SendForApproval"
    public string ConditionRule { get; private set; } // e.g. "TotalAmount > 5000"
    public string ResponseAction { get; private set; } // e.g. "CreateApprovalEntry", "ReleaseDocument"

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
/// Approval Entry. Mirrors Business Central table 454 "Approval Entry".
/// </summary>
public class ApprovalEntry : FullAuditedEntity<Guid>
{
    public string TableName { get; private set; }
    public Guid DocumentId { get; private set; }
    public string DocumentNo { get; private set; }
    public Guid SenderId { get; private set; }
    public Guid ApproverId { get; private set; }
    public string Status { get; private set; } // Created, Open, Approved, Rejected
    public decimal Amount { get; private set; }

    protected ApprovalEntry() { }

    public ApprovalEntry(
        Guid id,
        string tableName,
        Guid documentId,
        string documentNo,
        Guid senderId,
        Guid approverId,
        decimal amount
    )
        : base(id)
    {
        TableName = Check.NotNullOrWhiteSpace(tableName, nameof(tableName));
        DocumentId = documentId;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        SenderId = senderId;
        ApproverId = approverId;
        Status = "Open";
        Amount = amount;
    }

    public void Approve() => Status = "Approved";
    public void Reject() => Status = "Rejected";
}
