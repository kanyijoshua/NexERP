using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Workflows;

[Authorize(ErpPermissions.Workflows.Default)]
public class WorkflowAppService : ErpAppService, IWorkflowAppService
{
    private const string OpenStatus = "Open";

    private readonly IRepository<Workflow, Guid> _workflowRepository;
    private readonly IRepository<ApprovalEntry, Guid> _approvalEntryRepository;

    public WorkflowAppService(
        IRepository<Workflow, Guid> workflowRepository,
        IRepository<ApprovalEntry, Guid> approvalEntryRepository
    )
    {
        _workflowRepository = workflowRepository;
        _approvalEntryRepository = approvalEntryRepository;
    }

    public async Task<ListResultDto<WorkflowDto>> GetListAsync()
    {
        var workflows = await _workflowRepository.GetListAsync(includeDetails: true);

        return new ListResultDto<WorkflowDto>(
            ObjectMapper.Map<List<Workflow>, List<WorkflowDto>>(workflows.OrderBy(w => w.Code).ToList())
        );
    }

    public async Task<PagedResultDto<ApprovalEntryDto>> GetApprovalEntriesAsync(GetApprovalEntriesInput input)
    {
        var status = input.Status.IsNullOrWhiteSpace() ? OpenStatus : input.Status;

        var query = (await _approvalEntryRepository.GetQueryableAsync())
            .Where(a => a.Status == status)
            .WhereIf(input.OnlyMine && CurrentUser.Id.HasValue, a => a.ApproverId == CurrentUser.Id.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = input.Sorting.IsNullOrWhiteSpace()
            ? query.OrderByDescending(a => a.CreationTime)
            : query.OrderBy(input.Sorting);

        var entries = await AsyncExecuter.ToListAsync(query.PageBy(input));

        return new PagedResultDto<ApprovalEntryDto>(
            totalCount,
            ObjectMapper.Map<List<ApprovalEntry>, List<ApprovalEntryDto>>(entries)
        );
    }

    [Authorize(ErpPermissions.Workflows.Approve)]
    public async Task ApproveAsync(Guid id)
    {
        var entry = await _approvalEntryRepository.GetAsync(id);
        entry.Approve();
        await _approvalEntryRepository.UpdateAsync(entry);
    }

    [Authorize(ErpPermissions.Workflows.Approve)]
    public async Task RejectAsync(Guid id)
    {
        var entry = await _approvalEntryRepository.GetAsync(id);
        entry.Reject();
        await _approvalEntryRepository.UpdateAsync(entry);
    }

    [Authorize(ErpPermissions.Workflows.Manage)]
    public async Task EnableAsync(Guid id)
    {
        var workflow = await _workflowRepository.GetAsync(id);
        workflow.Enable();
        await _workflowRepository.UpdateAsync(workflow);
    }

    [Authorize(ErpPermissions.Workflows.Manage)]
    public async Task DisableAsync(Guid id)
    {
        var workflow = await _workflowRepository.GetAsync(id);
        workflow.Disable();
        await _workflowRepository.UpdateAsync(workflow);
    }
}
