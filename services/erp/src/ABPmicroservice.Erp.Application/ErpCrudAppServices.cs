using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Querying;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp;

/// <summary>
/// The list query every ERP list shares: the service's own filters, then the filter pane's
/// conditions (<see cref="IHasDynamicFilter"/>), counted, sorted and paged. Mirrors ABP's
/// <c>GetListAsync</c> step for step, with the dynamic filter added after the service's own.
/// </summary>
internal static class ErpListQuery
{
    public static IQueryable<TEntity> Filter<TEntity>(
        IQueryable<TEntity> query,
        object input,
        DateTime today,
        IReadOnlyDictionary<string, string> aliases
    )
    {
        return input is IHasDynamicFilter { DynamicFilter: { Length: > 0 } json }
            ? query.ApplyDynamicFilter(json, today, aliases)
            : query;
    }
}

/// <summary>A CRUD service whose list understands the filter pane's conditions.</summary>
public abstract class ErpCrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    : CrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    where TEntity : class, IEntity<TKey>
    where TEntityDto : IEntityDto<TKey>
{
    protected ErpCrudAppService(IRepository<TEntity, TKey> repository)
        : base(repository) { }

    /// <summary>
    /// List fields whose name differs from the entity property they filter, e.g. a DTO's
    /// <c>partyNo</c> that is the entity's <c>CustomerNo</c>.
    /// </summary>
    protected virtual IReadOnlyDictionary<string, string> DynamicFilterAliases => null;

    public override async Task<PagedResultDto<TEntityDto>> GetListAsync(TGetListInput input)
    {
        await CheckGetListPolicyAsync();

        var query = ErpListQuery.Filter(await CreateFilteredQueryAsync(input), input, Clock.Now, DynamicFilterAliases);
        var totalCount = await AsyncExecuter.CountAsync(query);

        var dtos = new List<TEntityDto>();
        if (totalCount > 0)
        {
            query = ApplyPaging(ApplySorting(query, input), input);
            dtos = await MapToGetListOutputDtosAsync(await AsyncExecuter.ToListAsync(query));
        }

        return new PagedResultDto<TEntityDto>(totalCount, dtos);
    }
}

/// <summary>A read-only service (ledgers) whose list understands the filter pane's conditions.</summary>
public abstract class ErpReadOnlyAppService<TEntity, TEntityDto, TKey, TGetListInput>
    : ReadOnlyAppService<TEntity, TEntityDto, TKey, TGetListInput>
    where TEntity : class, IEntity<TKey>
    where TEntityDto : IEntityDto<TKey>
{
    protected ErpReadOnlyAppService(IReadOnlyRepository<TEntity, TKey> repository)
        : base(repository) { }

    protected virtual IReadOnlyDictionary<string, string> DynamicFilterAliases => null;

    public override async Task<PagedResultDto<TEntityDto>> GetListAsync(TGetListInput input)
    {
        await CheckGetListPolicyAsync();

        var query = ErpListQuery.Filter(await CreateFilteredQueryAsync(input), input, Clock.Now, DynamicFilterAliases);
        var totalCount = await AsyncExecuter.CountAsync(query);

        var dtos = new List<TEntityDto>();
        if (totalCount > 0)
        {
            query = ApplyPaging(ApplySorting(query, input), input);
            dtos = await MapToGetListOutputDtosAsync(await AsyncExecuter.ToListAsync(query));
        }

        return new PagedResultDto<TEntityDto>(totalCount, dtos);
    }
}
