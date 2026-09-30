using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Inventory;

[Authorize(ErpPermissions.UnitsOfMeasure.Default)]
public class UnitOfMeasureAppService
    : ErpCrudAppService<
        UnitOfMeasure,
        UnitOfMeasureDto,
        Guid,
        GetUnitOfMeasureListInput,
        CreateUpdateUnitOfMeasureDto,
        CreateUpdateUnitOfMeasureDto
    >,
        IUnitOfMeasureAppService
{
    public UnitOfMeasureAppService(IRepository<UnitOfMeasure, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.UnitsOfMeasure.Default;
        GetListPolicyName = ErpPermissions.UnitsOfMeasure.Default;
        CreatePolicyName = ErpPermissions.UnitsOfMeasure.Create;
        UpdatePolicyName = ErpPermissions.UnitsOfMeasure.Update;
        DeletePolicyName = ErpPermissions.UnitsOfMeasure.Delete;
    }

    // Mapped by hand: the entity has private setters and guards, which AutoMapper cannot honour.
    public override async Task<UnitOfMeasureDto> CreateAsync(CreateUpdateUnitOfMeasureDto input)
    {
        await CheckCreatePolicyAsync();
        await EnsureCodeIsUniqueAsync(input.Code, null);

        var unit = new UnitOfMeasure(GuidGenerator.Create(), input.Code, input.Description);

        await Repository.InsertAsync(unit, autoSave: true);
        return await MapToGetOutputDtoAsync(unit);
    }

    public override async Task<UnitOfMeasureDto> UpdateAsync(Guid id, CreateUpdateUnitOfMeasureDto input)
    {
        await CheckUpdatePolicyAsync();

        var unit = await GetEntityByIdAsync(id);
        await EnsureCodeIsUniqueAsync(input.Code, id);

        unit.SetCode(input.Code);
        unit.SetDescription(input.Description);

        await Repository.UpdateAsync(unit, autoSave: true);
        return await MapToGetOutputDtoAsync(unit);
    }

    protected override async Task<IQueryable<UnitOfMeasure>> CreateFilteredQueryAsync(GetUnitOfMeasureListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            x => x.Code.ToLower().Contains(filter) || (x.Description != null && x.Description.ToLower().Contains(filter))
        );
    }

    protected override IQueryable<UnitOfMeasure> ApplyDefaultSorting(IQueryable<UnitOfMeasure> query)
    {
        return query.OrderBy(x => x.Code);
    }

    private async Task EnsureCodeIsUniqueAsync(string code, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.Code == code && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.Items.CodeAlreadyExists).WithData("code", code);
        }
    }
}
