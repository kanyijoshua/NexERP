using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Workflows;

[Authorize(ErpPermissions.Workflows.Default)]
public class WorkflowAppService : ErpAppService, IWorkflowAppService
{
    private readonly IRepository<Workflow, Guid> _workflowRepository;

    public WorkflowAppService(IRepository<Workflow, Guid> workflowRepository)
    {
        _workflowRepository = workflowRepository;
    }

    public async Task<ListResultDto<WorkflowDto>> GetListAsync()
    {
        var workflows = await _workflowRepository.GetListAsync(includeDetails: true);

        return new ListResultDto<WorkflowDto>(workflows.OrderBy(w => w.Code).Select(Map).ToList());
    }

    public async Task<WorkflowDto> GetAsync(Guid id)
    {
        return Map(await _workflowRepository.GetAsync(id));
    }

    [Authorize(ErpPermissions.Workflows.Manage)]
    public async Task<WorkflowDto> CreateAsync(CreateUpdateWorkflowDto input)
    {
        var code = input.Code.Trim().ToUpperInvariant();
        if (await _workflowRepository.AnyAsync(w => w.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.Approvals.WorkflowCodeAlreadyExists).WithData("code", code);
        }

        var workflow = new Workflow(GuidGenerator.Create(), code, input.Description, input.DocumentKind,
            input.MinimumAmount, input.ApproverLimitType, input.DueDays);
        workflow.RebuildSteps(GuidGenerator.Create);

        await _workflowRepository.InsertAsync(workflow, autoSave: true);
        return Map(workflow);
    }

    [Authorize(ErpPermissions.Workflows.Manage)]
    public async Task<WorkflowDto> UpdateAsync(Guid id, CreateUpdateWorkflowDto input)
    {
        var workflow = await _workflowRepository.GetAsync(id);

        // As in Business Central, an enabled workflow is read-only: requests may be in flight under its rules.
        if (workflow.Enabled)
        {
            throw new UserFriendlyException(L["Workflow:DisableBeforeEditing"]);
        }

        workflow.Update(input.Description, input.DocumentKind, input.MinimumAmount, input.ApproverLimitType, input.DueDays);
        workflow.RebuildSteps(GuidGenerator.Create);

        await _workflowRepository.UpdateAsync(workflow, autoSave: true);
        return Map(workflow);
    }

    [Authorize(ErpPermissions.Workflows.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var workflow = await _workflowRepository.GetAsync(id);
        if (workflow.Enabled)
        {
            throw new UserFriendlyException(L["Workflow:DisableBeforeEditing"]);
        }

        await _workflowRepository.DeleteAsync(workflow);
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

    private WorkflowDto Map(Workflow workflow)
    {
        var dto = ObjectMapper.Map<Workflow, WorkflowDto>(workflow);
        dto.Steps = dto.Steps.OrderBy(s => s.SequenceNo).ToList();
        return dto;
    }
}
