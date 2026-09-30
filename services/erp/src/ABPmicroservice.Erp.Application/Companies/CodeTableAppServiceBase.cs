using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// What every code table page shares: a unique upper-case code, a description, search on either,
/// and one permission group (Default to read, Create/Update/Delete to change). Entities are mapped
/// by hand on create and update; <see cref="ApplyAsync"/> takes everything beyond code and description.
/// </summary>
public abstract class CodeTableAppServiceBase<TEntity, TDto, TInput>
    : ErpCrudAppService<TEntity, TDto, Guid, GetCodeTableListInput, TInput, TInput>
    where TEntity : CodeTableEntity
    where TDto : CodeTableDto
    where TInput : CreateUpdateCodeTableDto
{
    protected CodeTableChecker CodeTableChecker => LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();

    /// <param name="permission">The Default permission of the group; ".Create", ".Update" and ".Delete" are appended.</param>
    protected CodeTableAppServiceBase(IRepository<TEntity, Guid> repository, string permission)
        : base(repository)
    {
        GetPolicyName = permission;
        GetListPolicyName = permission;
        CreatePolicyName = permission + ".Create";
        UpdatePolicyName = permission + ".Update";
        DeletePolicyName = permission + ".Delete";
    }

    protected abstract TEntity NewEntity(Guid id, TInput input);

    /// <summary>Everything beyond code and description; also where related codes and accounts are checked.</summary>
    protected virtual Task ApplyAsync(TEntity entity, TInput input) => Task.CompletedTask;

    public override async Task<TDto> CreateAsync(TInput input)
    {
        await CheckCreatePolicyAsync();
        await EnsureCodeIsUniqueAsync(input.Code, null);

        var entity = NewEntity(GuidGenerator.Create(), input);
        await ApplyAsync(entity, input);

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<TDto> UpdateAsync(Guid id, TInput input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await EnsureCodeIsUniqueAsync(input.Code, id);

        entity.SetCode(input.Code);
        entity.SetDescription(input.Description);
        await ApplyAsync(entity, input);

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<TEntity>> CreateFilteredQueryAsync(GetCodeTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query.WhereIf(
            !filter.IsNullOrEmpty(),
            x => x.Code.ToLower().Contains(filter) || (x.Description != null && x.Description.ToLower().Contains(filter))
        );
    }

    protected override IQueryable<TEntity> ApplyDefaultSorting(IQueryable<TEntity> query)
    {
        return query.OrderBy(x => x.Code);
    }

    private async Task EnsureCodeIsUniqueAsync(string code, Guid? exceptId)
    {
        var normalized = CodeTableEntity.NormalizeCode(code);
        if (await Repository.AnyAsync(x => x.Code == normalized && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeAlreadyExists).WithData("code", normalized);
        }
    }
}
