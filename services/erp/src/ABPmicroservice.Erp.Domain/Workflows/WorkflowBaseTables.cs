using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>Workflow User Group.</summary>
public class WorkflowUserGroup : CodeTableEntity
{
    protected WorkflowUserGroup() { }

    public WorkflowUserGroup(Guid id, string code, string description = null)
        : base(id, code, description) { }
}

/// <summary>Workflow User Group Member.</summary>
public class WorkflowUserGroupMember : CompanyEntity
{
    public string WorkflowUserGroupCode { get; private set; }
    public string UserName { get; private set; }

    public int SequenceNo { get; private set; }

    protected WorkflowUserGroupMember() { }

    public WorkflowUserGroupMember(Guid id, string workflowUserGroupCode, string userName)
        : base(id)
    {
        SetKey(workflowUserGroupCode, userName);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(string workflowUserGroupCode, string userName)
    {
        WorkflowUserGroupCode = Check.NotNullOrWhiteSpace(workflowUserGroupCode, nameof(workflowUserGroupCode), 20).Trim().ToUpperInvariant();
        UserName = Check.NotNullOrWhiteSpace(userName, nameof(userName), 50).Trim().ToUpperInvariant();
    }

    public void Set(int sequenceNo)
    {
        SequenceNo = sequenceNo;
    }
}

/// <summary>Approval Comment Line.</summary>
public class ApprovalCommentLine : CompanyEntity
{
    public long EntryNo { get; private set; }

    public int TableId { get; private set; }
    public ApprovalDocumentType DocumentType { get; private set; }
    public string DocumentNo { get; private set; }
    public string UserId { get; private set; }
    public DateTime? DateAndTime { get; private set; }
    public string Comment { get; private set; }

    protected ApprovalCommentLine() { }

    public ApprovalCommentLine(Guid id, long entryNo)
        : base(id)
    {
        SetKey(entryNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(long entryNo)
    {
        EntryNo = entryNo;
    }

    public void Set(
        int tableId,
        ApprovalDocumentType documentType,
        string documentNo,
        string userId,
        DateTime? dateAndTime,
        string comment
    )
    {
        TableId = tableId;
        DocumentType = documentType;
        DocumentNo = CodeTableEntity.NormalizeCode(Check.Length(documentNo, nameof(documentNo), 20));
        UserId = CodeTableEntity.NormalizeCode(Check.Length(userId, nameof(userId), 50));
        DateAndTime = dateAndTime;
        Comment = Check.Length(comment, nameof(comment), 80);
    }
}
