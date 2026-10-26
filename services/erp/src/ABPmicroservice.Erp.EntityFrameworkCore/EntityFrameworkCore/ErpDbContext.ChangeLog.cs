using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Auditing;
using ABPmicroservice.Erp.Exporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// The change log: while a save is still pending, every changed field of a table the company logs
/// becomes a <see cref="ChangeLogEntry"/> in the same save, so a change and its log commit together.
/// </summary>
public partial class ErpDbContext
{
    /// <summary>Bookkeeping columns every row has; logging them would only repeat the entry itself.</summary>
    private static readonly HashSet<string> UnloggedProperties =
    [
        "Id",
        "CompanyId",
        "TenantId",
        "ConcurrencyStamp",
        "ExtraProperties",
        "CreationTime",
        "CreatorId",
        "LastModificationTime",
        "LastModifierId",
        "IsDeleted",
        "DeleterId",
        "DeletionTime",
    ];

    private static readonly string[] KeyProperties = ["No", "Code", "EntryNo", "EntityName"];

    private async Task RecordChangeLogAsync(CancellationToken cancellationToken)
    {
        if (!HasLoggableChanges())
        {
            return;
        }

        var configuration = FindCachedChangeLogConfiguration(out var cache, out var key);
        if (configuration == null && cache != null)
        {
            var setup = await ChangeLogSetups.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            var tables = setup?.ChangeLogActivated == true
                ? await ChangeLogTables.AsNoTracking().ToListAsync(cancellationToken)
                : [];
            configuration = CacheChangeLogConfiguration(cache, key, setup, tables);
        }

        RecordChangeLog(configuration);
    }

    private void RecordChangeLog()
    {
        if (!HasLoggableChanges())
        {
            return;
        }

        var configuration = FindCachedChangeLogConfiguration(out var cache, out var key);
        if (configuration == null && cache != null)
        {
            var setup = ChangeLogSetups.AsNoTracking().FirstOrDefault();
            var tables = setup?.ChangeLogActivated == true ? ChangeLogTables.AsNoTracking().ToList() : [];
            configuration = CacheChangeLogConfiguration(cache, key, setup, tables);
        }

        RecordChangeLog(configuration);
    }

    /// <summary>Saves that change nothing a company could log (or happen outside a company) skip the lookup.</summary>
    private bool HasLoggableChanges() =>
        CurrentCompanyId != null
        && ChangeTracker.Entries().Any(e =>
            e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && e.Entity is not ChangeLogEntry
        );

    private ChangeLogConfiguration FindCachedChangeLogConfiguration(out ChangeLogConfigurationCache cache, out (Guid? TenantId, Guid CompanyId) key)
    {
        cache = LazyServiceProvider?.LazyGetService<ChangeLogConfigurationCache>();
        key = (CurrentTenantId, CurrentCompanyId ?? Guid.Empty);
        if (cache == null)
        {
            return ChangeLogConfiguration.Off;
        }

        return cache.TryGet(key.TenantId, key.CompanyId, out var configuration) ? configuration : null;
    }

    private static ChangeLogConfiguration CacheChangeLogConfiguration(
        ChangeLogConfigurationCache cache,
        (Guid? TenantId, Guid CompanyId) key,
        ChangeLogSetup setup,
        List<ChangeLogTable> tables
    )
    {
        var configuration = new ChangeLogConfiguration(
            setup?.ChangeLogActivated == true,
            tables.ToDictionary(t => t.EntityName, ChangeLogTableSettings.From, StringComparer.OrdinalIgnoreCase)
        );
        cache.Set(key.TenantId, key.CompanyId, configuration);
        return configuration;
    }

    private void RecordChangeLog(ChangeLogConfiguration configuration)
    {
        if (configuration == null || !configuration.LogsAnything)
        {
            return;
        }

        var registry = LazyServiceProvider?.LazyGetService<ErpEntityRegistry>();
        var currentUser = LazyServiceProvider?.LazyGetService<ICurrentUser>();
        var now = LazyServiceProvider?.LazyGetService<IClock>()?.Now ?? DateTime.Now;
        List<ChangeLogEntry> logged = null;

        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            var type = entry.State switch
            {
                EntityState.Added => ChangeLogEntryType.Insertion,
                EntityState.Modified => ChangeLogEntryType.Modification,
                EntityState.Deleted => ChangeLogEntryType.Deletion,
                _ => (ChangeLogEntryType?)null,
            };
            if (type == null || entry.Entity is ChangeLogEntry || entry.Entity is not IEntity<Guid> entity)
            {
                continue;
            }

            var clrType = entry.Metadata.ClrType;
            var definition = registry?.Find(clrType.Name);
            var entityName = definition != null && definition.EntityType == clrType ? definition.Name : clrType.Name;
            if (!configuration.Tables.TryGetValue(entityName, out var settings))
            {
                continue;
            }

            var primaryKey = FindPrimaryKey(entry);
            foreach (var property in entry.Properties)
            {
                var name = property.Metadata.Name;
                if (UnloggedProperties.Contains(name) || property.Metadata.IsShadowProperty() || !settings.Logs(type.Value, name))
                {
                    continue;
                }

                string oldValue = null, newValue = null;
                switch (type.Value)
                {
                    case ChangeLogEntryType.Insertion:
                        newValue = Format(property.CurrentValue);
                        if (newValue == null)
                        {
                            continue;
                        }
                        break;
                    case ChangeLogEntryType.Modification:
                        if (!property.IsModified)
                        {
                            continue;
                        }
                        oldValue = Format(property.OriginalValue);
                        newValue = Format(property.CurrentValue);
                        if (oldValue == newValue)
                        {
                            continue;
                        }
                        break;
                    default:
                        oldValue = Format(property.OriginalValue);
                        if (oldValue == null)
                        {
                            continue;
                        }
                        break;
                }

                logged ??= [];
                logged.Add(
                    new ChangeLogEntry(
                        GuidGenerator.Create(),
                        now,
                        currentUser?.Id,
                        currentUser?.UserName,
                        entityName,
                        entity.Id,
                        primaryKey,
                        name,
                        type.Value,
                        oldValue,
                        newValue
                    )
                );
            }
        }

        if (logged != null)
        {
            ChangeLogEntries.AddRange(logged);
        }
    }

    private static string FindPrimaryKey(EntityEntry entry)
    {
        foreach (var name in KeyProperties)
        {
            var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == name);
            if (property != null)
            {
                return Format(entry.State == EntityState.Deleted ? property.OriginalValue : property.CurrentValue);
            }
        }

        return null;
    }

    private static string Format(object value) => value switch
    {
        null => null,
        string s => s.Length == 0 ? null : s,
        DateTime d => d.TimeOfDay == TimeSpan.Zero
            ? d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : d.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        decimal m => m.ToString("0.#####", CultureInfo.InvariantCulture),
        byte[] => null,
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };
}
