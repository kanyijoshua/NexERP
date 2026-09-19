using System;
using System.Collections.Generic;
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
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class WorkflowStepDto : EntityDto<Guid>
{
    public int SequenceNo { get; set; }
    public string EventName { get; set; }
    public string ConditionRule { get; set; }
    public string ResponseAction { get; set; }
}

public class ApprovalEntryDto : EntityDto<Guid>
{
    public string TableName { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentNo { get; set; }
    public Guid SenderId { get; set; }
    public Guid ApproverId { get; set; }
    public string Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreationTime { get; set; }
}

public class GetApprovalEntriesInput : PagedAndSortedResultRequestDto
{
    /// <summary>Created, Open, Approved or Rejected. Defaults to Open.</summary>
    public string Status { get; set; }

    /// <summary>Only entries waiting for the current user.</summary>
    public bool OnlyMine { get; set; }
}

public interface IWorkflowAppService : IApplicationService
{
    Task<ListResultDto<WorkflowDto>> GetListAsync();

    /// <summary>Routed as GET /api/erp/workflow/approval-entries.</summary>
    Task<PagedResultDto<ApprovalEntryDto>> GetApprovalEntriesAsync(GetApprovalEntriesInput input);

    /// <summary>Approves an approval entry. Routed as POST /api/erp/workflow/{id}/approve.</summary>
    Task ApproveAsync(Guid id);

    /// <summary>Rejects an approval entry. Routed as POST /api/erp/workflow/{id}/reject.</summary>
    Task RejectAsync(Guid id);

    Task EnableAsync(Guid id);

    Task DisableAsync(Guid id);
}
