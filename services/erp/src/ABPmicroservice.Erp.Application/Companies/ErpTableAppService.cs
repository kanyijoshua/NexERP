using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// What the pages of the plain base tables share: one permission group (Default
/// to read, Create/Update/Delete to change) and the TableRelation checks. A table keyed by more
/// than a code derives from this; a code table derives from <see cref="CodeTableAppServiceBase{TEntity,TDto,TInput}"/>.
/// </summary>
public abstract class ErpTableAppService<TEntity, TDto, TListInput, TInput>
    : ErpCrudAppService<TEntity, TDto, Guid, TListInput, TInput, TInput>
    where TEntity : class, IEntity<Guid>
    where TDto : IEntityDto<Guid>
{
    protected CodeTableChecker CodeTableChecker => LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>();

    protected TableRelationChecker Relations => LazyServiceProvider.LazyGetRequiredService<TableRelationChecker>();

    /// <param name="permission">The Default permission of the group; ".Create", ".Update" and ".Delete" are appended.</param>
    protected ErpTableAppService(IRepository<TEntity, Guid> repository, string permission)
        : base(repository)
    {
        GetPolicyName = permission;
        GetListPolicyName = permission;
        CreatePolicyName = permission + ".Create";
        UpdatePolicyName = permission + ".Update";
        DeletePolicyName = permission + ".Delete";
    }
}
