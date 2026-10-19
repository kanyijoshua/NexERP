using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Procurement;

[Authorize(ErpPermissions.ProcurementPlans.Default)]
public class ProcurementPlanAppService
    : ErpCrudAppService<
        ProcurementPlan,
        ProcurementPlanDto,
        Guid,
        GetProcurementPlanListInput,
        CreateUpdateProcurementPlanDto,
        CreateUpdateProcurementPlanDto
    >,
        IProcurementPlanAppService
{
    public ProcurementPlanAppService(IRepository<ProcurementPlan, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.ProcurementPlans.Default;
        GetListPolicyName = ErpPermissions.ProcurementPlans.Default;
        CreatePolicyName = ErpPermissions.ProcurementPlans.Create;
        UpdatePolicyName = ErpPermissions.ProcurementPlans.Update;
        DeletePolicyName = ErpPermissions.ProcurementPlans.Delete;
    }

    public override async Task<ProcurementPlanDto> CreateAsync(CreateUpdateProcurementPlanDto input)
    {
        await CheckCreatePolicyAsync();

        if (await Repository.AnyAsync(x => x.Code == input.Code))
        {
            throw new BusinessException(ErpErrorCodes.Procurement.ProcurementPlanAlreadyExists).WithData("code", input.Code);
        }

        var plan = new ProcurementPlan(
            GuidGenerator.Create(),
            input.Code,
            input.Description,
            input.BudgetName,
            input.StartDate,
            input.EndDate
        );

        foreach (var lineInput in input.Lines)
        {
            plan.AddLine(
                lineInput.Id ?? GuidGenerator.Create(),
                lineInput.PlanItemNo,
                lineInput.Description,
                lineInput.ProcurementType,
                lineInput.ProcurementMethod,
                lineInput.Quantity,
                lineInput.UnitPrice,
                lineInput.UnitOfMeasure,
                lineInput.SourceOfFunds,
                lineInput.DepartmentCode,
                lineInput.Quarter
            );
        }

        await Repository.InsertAsync(plan, autoSave: true);
        return await MapToGetOutputDtoAsync(plan);
    }

    public override async Task<ProcurementPlanDto> UpdateAsync(Guid id, CreateUpdateProcurementPlanDto input)
    {
        await CheckUpdatePolicyAsync();

        var plan = await Repository.GetAsync(id, includeDetails: true);
        plan.Update(input.Description, input.BudgetName, input.StartDate, input.EndDate);

        // Update lines
        var existingLineIds = plan.Lines.Select(l => l.Id).ToList();
        var incomingLineIds = input.Lines.Where(l => l.Id.HasValue).Select(l => l.Id.Value).ToHashSet();

        foreach (var existingId in existingLineIds)
        {
            if (!incomingLineIds.Contains(existingId))
            {
                plan.RemoveLine(existingId);
            }
        }

        foreach (var lineInput in input.Lines)
        {
            if (lineInput.Id.HasValue && plan.Lines.Any(l => l.Id == lineInput.Id.Value))
            {
                var existingLine = plan.Lines.First(l => l.Id == lineInput.Id.Value);
                existingLine.Update(
                    lineInput.Description,
                    lineInput.ProcurementType,
                    lineInput.ProcurementMethod,
                    lineInput.Quantity,
                    lineInput.UnitPrice,
                    lineInput.UnitOfMeasure,
                    lineInput.SourceOfFunds,
                    lineInput.DepartmentCode,
                    lineInput.Quarter
                );
            }
            else
            {
                plan.AddLine(
                    GuidGenerator.Create(),
                    lineInput.PlanItemNo,
                    lineInput.Description,
                    lineInput.ProcurementType,
                    lineInput.ProcurementMethod,
                    lineInput.Quantity,
                    lineInput.UnitPrice,
                    lineInput.UnitOfMeasure,
                    lineInput.SourceOfFunds,
                    lineInput.DepartmentCode,
                    lineInput.Quarter
                );
            }
        }
        plan.RecalculateTotal();

        await Repository.UpdateAsync(plan, autoSave: true);
        return await MapToGetOutputDtoAsync(plan);
    }

    [Authorize(ErpPermissions.ProcurementPlans.Approve)]
    public async Task<ProcurementPlanDto> ApproveAsync(Guid id)
    {
        var plan = await Repository.GetAsync(id, includeDetails: true);
        plan.Approve();
        await Repository.UpdateAsync(plan, autoSave: true);
        return await MapToGetOutputDtoAsync(plan);
    }

    [Authorize(ErpPermissions.ProcurementPlans.Update)]
    public async Task<ProcurementPlanDto> ReopenAsync(Guid id)
    {
        var plan = await Repository.GetAsync(id, includeDetails: true);
        plan.Reopen();
        await Repository.UpdateAsync(plan, autoSave: true);
        return await MapToGetOutputDtoAsync(plan);
    }

    [Authorize(ErpPermissions.ProcurementPlans.Update)]
    public async Task<ProcurementPlanDto> CloseAsync(Guid id)
    {
        var plan = await Repository.GetAsync(id, includeDetails: true);
        plan.Close();
        await Repository.UpdateAsync(plan, autoSave: true);
        return await MapToGetOutputDtoAsync(plan);
    }

    protected override async Task<ProcurementPlan> GetEntityByIdAsync(Guid id)
    {
        return await Repository.GetAsync(id, includeDetails: true);
    }

    protected override async Task<IQueryable<ProcurementPlan>> CreateFilteredQueryAsync(GetProcurementPlanListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Code.Contains(input.Filter) || x.Description.Contains(input.Filter))
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value);
    }
}
