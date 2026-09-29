using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// Reads and writes the rows of one table for a configuration package, by key rather than by id.
/// Resolved as a closed generic, like the export's <see cref="EntitySource{TEntity}"/>, because the
/// table is only known at run time.
/// </summary>
public interface IConfigEntityStore
{
    /// <summary>The record whose key fields hold these values, or null. A table with no key has at most one record.</summary>
    Task<object> FindAsync(ConfigTableProfile profile, IReadOnlyDictionary<string, object> key);

    /// <summary>Every record's key text mapped to its id, to resolve and check relations in bulk.</summary>
    Task<Dictionary<string, Guid>> LoadKeysAsync(string keyField);

    /// <summary>A blank record, made through the constructor the database uses to load one.</summary>
    object CreateNew(Guid id);

    Task InsertAsync(object entity);

    Task UpdateAsync(object entity);

    Task<int> DeleteAllAsync();
}

public class ConfigEntityStore<TEntity> : IConfigEntityStore
    where TEntity : class, IEntity<Guid>
{
    private readonly IRepository<TEntity, Guid> _repository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public ConfigEntityStore(IRepository<TEntity, Guid> repository, IAsyncQueryableExecuter asyncExecuter)
    {
        _repository = repository;
        _asyncExecuter = asyncExecuter;
    }

    public async Task<object> FindAsync(ConfigTableProfile profile, IReadOnlyDictionary<string, object> key)
    {
        var queryable = await _repository.GetQueryableAsync();
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        Expression body = Expression.Constant(true);

        foreach (var fieldName in profile.KeyFields)
        {
            Expression member = Expression.Property(parameter, fieldName);
            var value = key.GetValueOrDefault(fieldName);

            // Codes match whatever their case, as they do in BC, and on PostgreSQL as on SQLite.
            if (member.Type == typeof(string) && value is string text)
            {
                member = Expression.Call(member, typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!);
                value = text.ToLowerInvariant();
            }

            body = Expression.AndAlso(body, Expression.Equal(member, Expression.Constant(value, member.Type)));
        }

        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        return await _asyncExecuter.FirstOrDefaultAsync(queryable.Where(predicate));
    }

    public async Task<Dictionary<string, Guid>> LoadKeysAsync(string keyField)
    {
        var queryable = await _repository.GetQueryableAsync();

        // e => new KeyValuePair<string, Guid>(e.Key, e.Id), built by hand since the key is chosen at run time.
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var constructor = typeof(KeyValuePair<string, Guid>).GetConstructor([typeof(string), typeof(Guid)])!;
        var selector = Expression.Lambda<Func<TEntity, KeyValuePair<string, Guid>>>(
            Expression.New(
                constructor,
                Expression.Property(parameter, keyField),
                Expression.Property(parameter, nameof(IEntity<Guid>.Id))
            ),
            parameter
        );

        var pairs = await _asyncExecuter.ToListAsync(queryable.Select(selector));

        var keys = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in pairs.Where(p => p.Key != null))
        {
            keys.TryAdd(pair.Key, pair.Value);
        }

        return keys;
    }

    public object CreateNew(Guid id)
    {
        var entity = (TEntity)Activator.CreateInstance(typeof(TEntity), nonPublic: true)!;
        ConfigEntityWriter.SetValue(entity, nameof(IEntity<Guid>.Id), id);
        return entity;
    }

    public async Task InsertAsync(object entity)
    {
        // Saved at once so that the next record in the same run can find this one by its key.
        await _repository.InsertAsync((TEntity)entity, autoSave: true);
    }

    public async Task UpdateAsync(object entity)
    {
        await _repository.UpdateAsync((TEntity)entity, autoSave: true);
    }

    public async Task<int> DeleteAllAsync()
    {
        var count = await _repository.GetCountAsync();
        await _repository.DeleteAsync(_ => true, autoSave: true);
        return (int)count;
    }
}

/// <summary>
/// Sets a property whatever its accessibility. Entities keep their setters private so that screens
/// go through their methods; a package deliberately writes the stored value as it stands, as BC's
/// RapidStart does with field validation switched off, and checks the value itself beforehand.
/// </summary>
public static class ConfigEntityWriter
{
    public static void SetValue(object entity, string propertyName, object value)
    {
        var property = FindWritable(entity.GetType(), propertyName)
            ?? throw new InvalidOperationException($"{entity.GetType().Name}.{propertyName} cannot be written.");

        property.SetValue(entity, value);
    }

    public static object GetValue(object entity, string propertyName)
    {
        return entity.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(entity);
    }

    /// <summary>The property as declared, since only there does a private setter show up.</summary>
    private static PropertyInfo FindWritable(Type type, string propertyName)
    {
        for (var current = type; current != null; current = current.BaseType)
        {
            var property = current.GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly
            );

            if (property?.GetSetMethod(nonPublic: true) != null)
            {
                return property;
            }
        }

        return null;
    }
}
