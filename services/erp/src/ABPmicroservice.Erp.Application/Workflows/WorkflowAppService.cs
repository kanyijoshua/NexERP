using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Workflows;

public class WorkflowAppService : ApplicationService
{
    private readonly IRepository<Workflow, Guid> _workflowRepository;
    private readonly IRepository<ApprovalEntry, Guid> _approvalEntryRepository;
    private readonly WorkflowEngine _workflowEngine;

    public WorkflowAppService(
        IRepository<Workflow, Guid> workflowRepository,
        IRepository<ApprovalEntry, Guid> approvalEntryRepository,
        WorkflowEngine workflowEngine
    )
    {
        _workflowRepository = workflowRepository;
        _approvalEntryRepository = approvalEntryRepository;
        _workflowEngine = workflowEngine;
    }

    [Authorize(ErpPermissions.Workflows.Default)]
    public async Task<List<Workflow>> GetWorkflowsAsync()
    {
        return await _workflowRepository.GetListAsync(includeDetails: true);
    }

    [Authorize(ErpPermissions.Workflows.Default)]
    public async Task<List<ApprovalEntry>> GetPendingApprovalsAsync()
    {
        return await _approvalEntryRepository.GetListAsync(a => a.Status == "Open");
    }

    [Authorize(ErpPermissions.Workflows.Approve)]
    public async Task ApproveAsync(Guid approvalEntryId)
    {
        var entry = await _approvalEntryRepository.GetAsync(approvalEntryId);
        entry.Approve();
        await _approvalEntryRepository.UpdateAsync(entry);
    }

    [Authorize(ErpPermissions.Workflows.Approve)]
    public async Task RejectAsync(Guid approvalEntryId)
    {
        var entry = await _approvalEntryRepository.GetAsync(approvalEntryId);
        entry.Reject();
        await _approvalEntryRepository.UpdateAsync(entry);
    }
}
