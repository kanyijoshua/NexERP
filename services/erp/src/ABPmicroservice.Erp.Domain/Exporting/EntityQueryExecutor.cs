using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// Reads rows out of any registered table. One implementation serves the export dialog, the
/// integration API and the webhook payloads, so all three see the same data and the same filters.
/// </summary>
public interface IEntityQueryExecutor
{
    Task<EntityQueryResult> QueryAsync(EntityQueryRequest request);
}

public class EntityQueryExecutor : IEntityQueryExecutor, ITransientDependency
{
    private readonly ErpEntityRegistry _registry;
    private readonly IServiceProvider _serviceProvider;

    public EntityQueryExecutor(ErpEntityRegistry registry, IServiceProvider serviceProvider)
    {
        _registry = registry;
        _serviceProvider = serviceProvider;
    }

    public async Task<EntityQueryResult> QueryAsync(EntityQueryRequest request)
    {
        Check.NotNull(request, nameof(request));

        var definition = _registry.Get(request.EntityName);

        // The entity type is only known at run time, so the typed work happens in a closed
        // generic helper rather than through reflection on every row.
        var source = (IEntitySource)
            _serviceProvider.GetRequiredService(typeof(EntitySource<>).MakeGenericType(definition.EntityType));

        return await source.QueryAsync(definition, request);
    }
}

internal interface IEntitySource
{
    Task<EntityQueryResult> QueryAsync(ErpEntityDefinition definition, EntityQueryRequest request);
}

internal class EntitySource<TEntity> : IEntitySource
    where TEntity : class, IEntity
{
    private readonly IReadOnlyRepository<TEntity> _repository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public EntitySource(IReadOnlyRepository<TEntity> repository, IAsyncQueryableExecuter asyncExecuter)
    {
        _repository = repository;
        _asyncExecuter = asyncExecuter;
    }

    public async Task<EntityQueryResult> QueryAsync(ErpEntityDefinition definition, EntityQueryRequest request)
    {
        var fields = ResolveFields(definition, request);

        var queryable = await _repository.GetQueryableAsync();
        queryable = queryable.Where(EntityFilterExpressionBuilder.Build<TEntity>(definition, request.Filters));

        var totalCount = await _asyncExecuter.CountAsync(queryable);

        queryable = ApplyOrdering(queryable, definition, request);

        var take = Math.Clamp(
            request.MaxResultCount <= 0 ? 1000 : request.MaxResultCount,
            1,
            ErpDomainConsts.MaxExportRowCount
        );

        queryable = queryable.Skip(Math.Max(0, request.SkipCount)).Take(take);

        var entities = await _asyncExecuter.ToListAsync(queryable);

        return new EntityQueryResult
        {
            TotalCount = totalCount,
            Fields = fields,
            Items = entities.Select(entity => Project(entity, fields)).ToList(),
        };
    }

    private static List<ErpEntityField> ResolveFields(ErpEntityDefinition definition, EntityQueryRequest request)
    {
        if (request.Fields == null || request.Fields.Count == 0)
        {
            return definition.DefaultFields.ToList();
        }

        return request.Fields.Select(definition.GetField).ToList();
    }

    private static IQueryable<TEntity> ApplyOrdering(
        IQueryable<TEntity> queryable,
        ErpEntityDefinition definition,
        EntityQueryRequest request
    )
    {
        if (request.OrderBy.IsNullOrWhiteSpace())
        {
            return queryable;
        }

        var field = definition.GetField(request.OrderBy);
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var member = Expression.Property(parameter, field.Property);
        var selector = Expression.Lambda(member, parameter);

        var method = request.Descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

        var call = Expression.Call(
            typeof(Queryable),
            method,
            [typeof(TEntity), field.Property.PropertyType],
            queryable.Expression,
            Expression.Quote(selector)
        );

        return queryable.Provider.CreateQuery<TEntity>(call);
    }

    /// <summary>
    /// Enums leave as their name rather than their number, so an exported file and a JSON payload
    /// both read as "Invoice" instead of "2".
    /// </summary>
    private static Dictionary<string, object> Project(TEntity entity, List<ErpEntityField> fields)
    {
        var row = new Dictionary<string, object>(fields.Count);

        foreach (var field in fields)
        {
            var value = field.GetValue(entity);
            row[field.Name] = value != null && field.ClrType.IsEnum ? value.ToString() : value;
        }

        return row;
    }
}
