using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Workflows;

public class WorkflowDto : EntityDto<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public bool Enabled { get; set; }
    public ApprovalDocumentKind DocumentKind { get; set; }
    public decimal MinimumAmount { get; set; }
    public ApproverLimitType ApproverLimitType { get; set; }
    public int DueDays { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class WorkflowStepDto : EntityDto<Guid>
{
    public int SequenceNo { get; set; }
    public string EventName { get; set; }
    public string ConditionRule { get; set; }
    public string ResponseAction { get; set; }
}

public class CreateUpdateWorkflowDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxWorkflowCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public ApprovalDocumentKind DocumentKind { get; set; }

    /// <summary>Only documents of at least this amount need approval. Zero means all.</summary>
    [Range(0, double.MaxValue)]
    public decimal MinimumAmount { get; set; }

    public ApproverLimitType ApproverLimitType { get; set; }

    [Range(0, 3650)]
    public int DueDays { get; set; }
}

public class ApprovalEntryDto : EntityDto<Guid>
{
    public ApprovalDocumentKind DocumentKind { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentNo { get; set; }
    public string WorkflowCode { get; set; }
    public int SequenceNo { get; set; }
    public Guid SenderId { get; set; }
    public string SenderUserName { get; set; }
    public Guid ApproverId { get; set; }
    public string ApproverUserName { get; set; }
    public ApprovalStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public string Comment { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastStatusChangeTime { get; set; }

    /// <summary>The current user may approve, reject or delegate this entry now.</summary>
    public bool CanAct { get; set; }
}

public class GetApprovalEntriesInput : PagedAndSortedResultRequestDto
{
    /// <summary>Defaults to Open unless a document is given or AllStatuses is set.</summary>
    public ApprovalStatus? Status { get; set; }

    /// <summary>Every status: the history views ("sent by me", the full log).</summary>
    public bool AllStatuses { get; set; }

    /// <summary>Only entries where the current user is the approver ("Requests to Approve").</summary>
    public bool OnlyMine { get; set; }

    /// <summary>Only entries the current user sent ("Requests Sent for Approval").</summary>
    public bool SentByMe { get; set; }

    /// <summary>All entries of one document, whatever their status (its approval history).</summary>
    public Guid? DocumentId { get; set; }
}

public class ApprovalCommentInput
{
    [StringLength(ErpDomainConsts.MaxCommentLength)]
    public string Comment { get; set; }
}

public class ApprovalRequestResultDto
{
    public bool AutoApproved { get; set; }
    public int ApproverCount { get; set; }
    public string FirstApproverUserName { get; set; }
}

public interface IWorkflowAppService : IApplicationService
{
    Task<ListResultDto<WorkflowDto>> GetListAsync();

    Task<WorkflowDto> GetAsync(Guid id);

    Task<WorkflowDto> CreateAsync(CreateUpdateWorkflowDto input);

    Task<WorkflowDto> UpdateAsync(Guid id, CreateUpdateWorkflowDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Routed as POST /api/erp/workflow/{id}/enable.</summary>
    Task EnableAsync(Guid id);

    /// <summary>Routed as POST /api/erp/workflow/{id}/disable.</summary>
    Task DisableAsync(Guid id);
}

/// <summary>What an approver does with requests. BC: the "Requests to Approve" page.</summary>
public interface IApprovalEntryAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/approval-entry.</summary>
    Task<PagedResultDto<ApprovalEntryDto>> GetListAsync(GetApprovalEntriesInput input);

    /// <summary>How many requests wait for the current user. Routed as GET /api/erp/approval-entry/my-open-count.</summary>
    Task<int> GetMyOpenCountAsync();

    /// <summary>Routed as POST /api/erp/approval-entry/{id}/approve.</summary>
    Task ApproveAsync(Guid id, ApprovalCommentInput input);

    /// <summary>Routed as POST /api/erp/approval-entry/{id}/reject.</summary>
    Task RejectAsync(Guid id, ApprovalCommentInput input);

    /// <summary>Hands the request to the approver's substitute. Routed as POST /api/erp/approval-entry/{id}/delegate.</summary>
    Task DelegateAsync(Guid id);
}

public class ApprovalUserSetupDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public Guid? ApproverUserId { get; set; }
    public string ApproverUserName { get; set; }
    public Guid? SubstituteUserId { get; set; }
    public string SubstituteUserName { get; set; }
    public decimal SalesAmountApprovalLimit { get; set; }
    public bool UnlimitedSalesApproval { get; set; }
    public decimal PurchaseAmountApprovalLimit { get; set; }
    public bool UnlimitedPurchaseApproval { get; set; }
    public bool IsApprovalAdministrator { get; set; }
}

public class CreateUpdateApprovalUserSetupDto
{
    /// <summary>The identity user this row is about. Fixed once created.</summary>
    public Guid UserId { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxUserNameLength)]
    public string UserName { get; set; }

    /// <summary>Must be a user that already has an approval user setup row.</summary>
    public Guid? ApproverUserId { get; set; }

    public Guid? SubstituteUserId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SalesAmountApprovalLimit { get; set; }

    public bool UnlimitedSalesApproval { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PurchaseAmountApprovalLimit { get; set; }

    public bool UnlimitedPurchaseApproval { get; set; }
    public bool IsApprovalAdministrator { get; set; }
}

public interface IApprovalUserSetupAppService
    : ICrudAppService<
        ApprovalUserSetupDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateApprovalUserSetupDto,
        CreateUpdateApprovalUserSetupDto
    > { }
