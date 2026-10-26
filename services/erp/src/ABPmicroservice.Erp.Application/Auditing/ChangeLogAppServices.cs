using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Auditing;

/// <summary>The tables the change log can watch and their fields, read from the entity registry.</summary>
public class ChangeLogTableCatalog : Volo.Abp.DependencyInjection.ITransientDependency
{
    private static readonly HashSet<string> Bookkeeping =
    [
        "Id", "CompanyId", "TenantId", "ConcurrencyStamp", "ExtraProperties", "CreationTime", "CreatorId",
        "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime",
    ];

    private readonly ErpEntityRegistry _registry;

    public ChangeLogTableCatalog(ErpEntityRegistry registry)
    {
        _registry = registry;
    }

    public List<ChangeLogLoggableTableDto> GetAll() =>
        _registry.GetAll().Select(d => new ChangeLogLoggableTableDto { EntityName = d.Name, Fields = FieldsOf(d.EntityType) }).ToList();

    /// <summary>The registry name of a table, as typed; throws for a table the registry does not know.</summary>
    public ErpEntityDefinition Get(string entityName) =>
        _registry.Find(entityName?.Trim())
        ?? _registry.GetAll().FirstOrDefault(d => string.Equals(d.Name, entityName?.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? throw new BusinessException(ErpErrorCodes.Auditing.UnknownTable).WithData("table", entityName ?? "");

    public void EnsureFieldsExist(ErpEntityDefinition table, IEnumerable<string> fields)
    {
        var known = FieldsOf(table.EntityType).ToHashSet(StringComparer.Ordinal);
        var unknown = (fields ?? []).Where(f => !f.IsNullOrWhiteSpace() && !known.Contains(f.Trim())).ToList();
        if (unknown.Count > 0)
        {
            throw new BusinessException(ErpErrorCodes.Auditing.UnknownField)
                .WithData("table", table.Name)
                .WithData("fields", string.Join(", ", unknown));
        }
    }

    /// <summary>Columns of the table: settable scalar properties, without the bookkeeping every row has.</summary>
    private static List<string> FieldsOf(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite || p.GetSetMethod(nonPublic: true) != null)
            .Where(p => !Bookkeeping.Contains(p.Name))
            .Where(p => p.PropertyType == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
            .Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string))
            .Select(p => p.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();
}

/// <summary>Change Log Setup: one record per company, created on first read.</summary>
[Authorize(ErpPermissions.ChangeLog.Manage)]
public class ChangeLogSetupAppService : ErpAppService, IChangeLogSetupAppService
{
    private readonly IRepository<ChangeLogSetup, Guid> _repository;
    private readonly ChangeLogConfigurationCache _cache;
    private readonly ChangeLogTableCatalog _catalog;

    public ChangeLogSetupAppService(
        IRepository<ChangeLogSetup, Guid> repository,
        ChangeLogConfigurationCache cache,
        ChangeLogTableCatalog catalog
    )
    {
        _repository = repository;
        _cache = cache;
        _catalog = catalog;
    }

    public async Task<ChangeLogSetupDto> GetAsync()
    {
        var setup = await GetOrCreateAsync();
        return new ChangeLogSetupDto { ChangeLogActivated = setup.ChangeLogActivated };
    }

    public async Task<ChangeLogSetupDto> UpdateAsync(ChangeLogSetupDto input)
    {
        var setup = await GetOrCreateAsync();
        setup.SetActivated(input.ChangeLogActivated);
        await _repository.UpdateAsync(setup, autoSave: true);
        _cache.Invalidate(CurrentTenant.Id, CurrentCompany.Id ?? Guid.Empty);
        return new ChangeLogSetupDto { ChangeLogActivated = setup.ChangeLogActivated };
    }

    public Task<List<ChangeLogLoggableTableDto>> GetLoggableTablesAsync() => Task.FromResult(_catalog.GetAll());

    private async Task<ChangeLogSetup> GetOrCreateAsync() =>
        await _repository.FirstOrDefaultAsync()
        ?? await _repository.InsertAsync(new ChangeLogSetup(GuidGenerator.Create()), autoSave: true);
}

/// <summary>The tables the change log watches, and which changes of each.</summary>
[Authorize(ErpPermissions.ChangeLog.Manage)]
public class ChangeLogTableAppService
    : ErpCrudAppService<ChangeLogTable, ChangeLogTableDto, Guid, GetChangeLogTableListInput, CreateUpdateChangeLogTableDto, CreateUpdateChangeLogTableDto>,
        IChangeLogTableAppService
{
    private readonly ChangeLogConfigurationCache _cache;
    private readonly ChangeLogTableCatalog _catalog;

    public ChangeLogTableAppService(
        IRepository<ChangeLogTable, Guid> repository,
        ChangeLogConfigurationCache cache,
        ChangeLogTableCatalog catalog
    )
        : base(repository)
    {
        _cache = cache;
        _catalog = catalog;
        GetPolicyName = ErpPermissions.ChangeLog.Manage;
        GetListPolicyName = ErpPermissions.ChangeLog.Manage;
        CreatePolicyName = ErpPermissions.ChangeLog.Manage;
        UpdatePolicyName = ErpPermissions.ChangeLog.Manage;
        DeletePolicyName = ErpPermissions.ChangeLog.Manage;
    }

    public override async Task<ChangeLogTableDto> CreateAsync(CreateUpdateChangeLogTableDto input)
    {
        await CheckCreatePolicyAsync();
        var definition = _catalog.Get(input.EntityName);
        await EnsureUniqueAsync(definition.Name, null);

        var table = new ChangeLogTable(GuidGenerator.Create(), definition.Name);
        Apply(table, definition, input);

        await Repository.InsertAsync(table, autoSave: true);
        InvalidateCache();
        return await MapToGetOutputDtoAsync(table);
    }

    public override async Task<ChangeLogTableDto> UpdateAsync(Guid id, CreateUpdateChangeLogTableDto input)
    {
        await CheckUpdatePolicyAsync();
        var table = await GetEntityByIdAsync(id);
        Apply(table, _catalog.Get(table.EntityName), input);

        await Repository.UpdateAsync(table, autoSave: true);
        InvalidateCache();
        return await MapToGetOutputDtoAsync(table);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await base.DeleteAsync(id);
        InvalidateCache();
    }

    protected override async Task<IQueryable<ChangeLogTable>> CreateFilteredQueryAsync(GetChangeLogTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), x => x.EntityName.ToLower().Contains(filter));
    }

    protected override IQueryable<ChangeLogTable> ApplyDefaultSorting(IQueryable<ChangeLogTable> query) =>
        query.OrderBy(x => x.EntityName);

    private void Apply(ChangeLogTable table, ErpEntityDefinition definition, CreateUpdateChangeLogTableDto input)
    {
        _catalog.EnsureFieldsExist(definition, input.TrackedFields);
        table.Set(input.LogInsertion, input.LogModification, input.LogDeletion, input.TrackedFields);
    }

    private async Task EnsureUniqueAsync(string entityName, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.EntityName == entityName && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.Auditing.ChangeLogTableAlreadyExists).WithData("table", entityName);
        }
    }

    private void InvalidateCache() =>
        _cache.Invalidate(CurrentTenant.Id, LazyServiceProvider.LazyGetRequiredService<ICurrentCompany>().Id ?? Guid.Empty);
}

/// <summary>The change log itself: who changed which field of which record, and when.</summary>
[Authorize(ErpPermissions.ChangeLog.Default)]
public class ChangeLogEntryAppService
    : ErpReadOnlyAppService<ChangeLogEntry, ChangeLogEntryDto, Guid, GetChangeLogEntryListInput>,
        IChangeLogEntryAppService
{
    private readonly IRepository<ChangeLogEntry, Guid> _repository;

    public ChangeLogEntryAppService(IRepository<ChangeLogEntry, Guid> repository)
        : base(repository)
    {
        _repository = repository;
        GetPolicyName = ErpPermissions.ChangeLog.Default;
        GetListPolicyName = ErpPermissions.ChangeLog.Default;
    }

    [Authorize(ErpPermissions.ChangeLog.Manage)]
    public async Task<PurgeChangeLogEntriesResultDto> PurgeEntriesAsync(PurgeChangeLogEntriesInput input)
    {
        var before = input.Before.Date;
        var entityName = input.EntityName?.Trim();
        var entries = await _repository.GetListAsync(x =>
            x.ChangedAt < before && (entityName == null || entityName == "" || x.EntityName == entityName));

        await _repository.DeleteManyAsync(entries, autoSave: true);
        return new PurgeChangeLogEntriesResultDto { Deleted = entries.Count };
    }

    protected override async Task<IQueryable<ChangeLogEntry>> CreateFilteredQueryAsync(GetChangeLogEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var to = input.ToDate?.Date.AddDays(1);

        return query
            .WhereIf(!input.EntityName.IsNullOrWhiteSpace(), x => x.EntityName == input.EntityName.Trim())
            .WhereIf(input.RecordId.HasValue, x => x.RecordId == input.RecordId.Value)
            .WhereIf(!input.FieldName.IsNullOrWhiteSpace(), x => x.FieldName == input.FieldName.Trim())
            .WhereIf(input.TypeOfChange.HasValue, x => x.TypeOfChange == input.TypeOfChange.Value)
            .WhereIf(input.FromDate.HasValue, x => x.ChangedAt >= input.FromDate.Value.Date)
            .WhereIf(to.HasValue, x => x.ChangedAt < to.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.EntityName.ToLower().Contains(filter)
                    || x.FieldName.ToLower().Contains(filter)
                    || (x.PrimaryKey != null && x.PrimaryKey.ToLower().Contains(filter))
                    || (x.UserName != null && x.UserName.ToLower().Contains(filter))
                    || (x.OldValue != null && x.OldValue.ToLower().Contains(filter))
                    || (x.NewValue != null && x.NewValue.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<ChangeLogEntry> ApplyDefaultSorting(IQueryable<ChangeLogEntry> query) =>
        query.OrderByDescending(x => x.ChangedAt).ThenBy(x => x.EntityName).ThenBy(x => x.FieldName);
}
