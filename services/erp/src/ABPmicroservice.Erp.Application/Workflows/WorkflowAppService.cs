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
    private readonly IRepository<ApprovalUserSetup, Guid> _userSetupRepository;
    private readonly IRepository<WorkflowUserGroup, Guid> _userGroupRepository;

    public WorkflowAppService(
        IRepository<Workflow, Guid> workflowRepository,
        IRepository<ApprovalUserSetup, Guid> userSetupRepository,
        IRepository<WorkflowUserGroup, Guid> userGroupRepository
    )
    {
        _workflowRepository = workflowRepository;
        _userSetupRepository = userSetupRepository;
        _userGroupRepository = userGroupRepository;
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
        var code = await CheckNewCodeAsync(input.Code);

        var workflow = new Workflow(GuidGenerator.Create(), code, input.Description, input.DocumentKind,
            input.MinimumAmount, input.ApproverLimitType, input.DueDays);
        await ApplyApproversAsync(workflow, input);
        ApplySteps(workflow, input);

        await _workflowRepository.InsertAsync(workflow, autoSave: true);
        return Map(workflow);
    }

    [Authorize(ErpPermissions.Workflows.Manage)]
    public async Task<WorkflowDto> UpdateAsync(Guid id, CreateUpdateWorkflowDto input)
    {
        var workflow = await _workflowRepository.GetAsync(id);

        // An enabled workflow is read-only: requests may be in flight under its rules.
        if (workflow.Enabled)
        {
            throw new UserFriendlyException(L["Workflow:DisableBeforeEditing"]);
        }

        workflow.Update(input.Description, input.DocumentKind, input.MinimumAmount, input.ApproverLimitType, input.DueDays);
        await ApplyApproversAsync(workflow, input);
        ApplySteps(workflow, input);

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

    public Task<ListResultDto<WorkflowTemplateDto>> GetTemplatesAsync()
    {
        var templates = WorkflowTemplates.All
            .Select(t => new WorkflowTemplateDto
            {
                Code = t.Code,
                Description = t.Description,
                DocumentKind = t.DocumentKind,
                ApproverLimitType = t.ApproverLimitType,
                ApproverType = t.ApproverType,
                MinimumAmount = t.MinimumAmount,
                DueDays = t.DueDays,
                DelegateAfterDays = t.DelegateAfterDays,
            })
            .ToList();

        return Task.FromResult(new ListResultDto<WorkflowTemplateDto>(templates));
    }

    [Authorize(ErpPermissions.Workflows.Manage)]
    public async Task<WorkflowDto> NewFromTemplateAsync(NewWorkflowFromTemplateDto input)
    {
        var template = WorkflowTemplates.Find(input.TemplateCode)
            ?? throw new BusinessException(ErpErrorCodes.Approvals.WorkflowTemplateNotFound).WithData("code", input.TemplateCode);
        var code = await CheckNewCodeAsync(input.Code);

        var groupCode = template.ApproverType == WorkflowApproverType.WorkflowUserGroup
            ? await CheckUserGroupAsync(input.WorkflowUserGroupCode)
            : null;

        var workflow = template.CreateWorkflow(GuidGenerator.Create(), code, input.Description, groupCode, GuidGenerator.Create);
        await _workflowRepository.InsertAsync(workflow, autoSave: true);
        return Map(workflow);
    }

    [Authorize(ErpPermissions.Workflows.Manage)]
    public async Task<WorkflowDto> CopyAsync(Guid id, CopyWorkflowDto input)
    {
        var source = await _workflowRepository.GetAsync(id);
        var code = await CheckNewCodeAsync(input.Code);

        var copy = source.CopyAs(GuidGenerator.Create(), code, input.Description, GuidGenerator.Create);
        await _workflowRepository.InsertAsync(copy, autoSave: true);
        return Map(copy);
    }

    private async Task<string> CheckNewCodeAsync(string input)
    {
        var code = input.Trim().ToUpperInvariant();
        if (await _workflowRepository.AnyAsync(w => w.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.Approvals.WorkflowCodeAlreadyExists).WithData("code", code);
        }

        return code;
    }

    private async Task<string> CheckUserGroupAsync(string groupCode)
    {
        if (groupCode.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Approvals.WorkflowUserGroupRequired);
        }

        var code = groupCode.Trim().ToUpperInvariant();
        if (!await _userGroupRepository.AnyAsync(g => g.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.Approvals.WorkflowUserGroupNotFound).WithData("group", code);
        }

        return code;
    }

    private async Task ApplyApproversAsync(Workflow workflow, CreateUpdateWorkflowDto input)
    {
        var groupCode = input.ApproverType == WorkflowApproverType.WorkflowUserGroup
            ? await CheckUserGroupAsync(input.WorkflowUserGroupCode)
            : null;

        string specificName = null;
        var specificNeeded = input.ApproverType == WorkflowApproverType.Approver
            && input.ApproverLimitType == ApproverLimitType.SpecificApprover;
        if (specificNeeded && input.SpecificApproverUserId.HasValue)
        {
            var setup = await _userSetupRepository.FirstOrDefaultAsync(s => s.UserId == input.SpecificApproverUserId.Value)
                ?? throw new BusinessException(ErpErrorCodes.Approvals.UserNotInApprovalSetup).WithData("user", input.SpecificApproverUserId.Value);
            specificName = setup.UserName;
        }

        workflow.SetApprovers(input.ApproverType, groupCode, input.SpecificApproverUserId, specificName);
        workflow.SetDelegateAfterDays(input.DelegateAfterDays);
    }

    // Steps sent by the page are kept as written; none means the standard steps for the settings.
    private void ApplySteps(Workflow workflow, CreateUpdateWorkflowDto input)
    {
        if (input.Steps != null && input.Steps.Count > 0)
        {
            workflow.SetSteps(input.Steps.OrderBy(s => s.SequenceNo).Select(s => (s.EventName, s.ConditionRule, s.ResponseAction)), GuidGenerator.Create);
        }
        else
        {
            workflow.RebuildSteps(GuidGenerator.Create);
        }
    }

    private WorkflowDto Map(Workflow workflow)
    {
        var dto = ObjectMapper.Map<Workflow, WorkflowDto>(workflow);
        dto.Steps = dto.Steps.OrderBy(s => s.SequenceNo).ToList();
        return dto;
    }
}
