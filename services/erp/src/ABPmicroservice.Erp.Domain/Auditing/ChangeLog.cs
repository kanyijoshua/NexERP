using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp.Auditing;

/// <summary>Change Log Setup: one row per company with the switch that turns logging on.</summary>
public class ChangeLogSetup : CompanyEntity
{
    public bool ChangeLogActivated { get; private set; }

    protected ChangeLogSetup() { }

    public ChangeLogSetup(Guid id)
        : base(id) { }

    public void SetActivated(bool activated) => ChangeLogActivated = activated;
}

/// <summary>
/// A table whose changes are logged, and which changes: insertions, modifications and deletions
/// each log all fields, the fields listed in <see cref="TrackedFields"/>, or nothing.
/// <see cref="EntityName"/> is a table name of the entity registry, the same names data export uses.
/// </summary>
public class ChangeLogTable : CompanyEntity
{
    public const int MaxTrackedFieldsLength = 2000;

    public string EntityName { get; private set; }
    public ChangeLogFieldFilterType LogInsertion { get; private set; }
    public ChangeLogFieldFilterType LogModification { get; private set; }
    public ChangeLogFieldFilterType LogDeletion { get; private set; }

    /// <summary>Comma-separated field names used where a type of change logs "Some Fields".</summary>
    public string TrackedFields { get; private set; }

    protected ChangeLogTable() { }

    public ChangeLogTable(Guid id, string entityName)
        : base(id)
    {
        EntityName = Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxNameLength).Trim();
    }

    public void Set(
        ChangeLogFieldFilterType logInsertion,
        ChangeLogFieldFilterType logModification,
        ChangeLogFieldFilterType logDeletion,
        IEnumerable<string> trackedFields
    )
    {
        LogInsertion = logInsertion;
        LogModification = logModification;
        LogDeletion = logDeletion;

        var fields = (trackedFields ?? []).Where(f => !f.IsNullOrWhiteSpace()).Select(f => f.Trim()).Distinct(StringComparer.Ordinal).ToList();
        TrackedFields = fields.Count == 0 ? null : Check.Length(string.Join(",", fields), nameof(trackedFields), MaxTrackedFieldsLength);
    }

    public IReadOnlyList<string> GetTrackedFields() =>
        TrackedFields.IsNullOrWhiteSpace() ? [] : TrackedFields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

/// <summary>
/// One changed field of one record: who changed it, when, and the value before and after.
/// Written in the same transaction as the change itself; only the clean-up deletes it.
/// </summary>
public class ChangeLogEntry : CompanyBasicEntity
{
    public const int MaxValueLength = 250;
    public const int MaxFieldNameLength = 100;
    public const int MaxPrimaryKeyLength = 100;

    public DateTime ChangedAt { get; private set; }
    public Guid? UserId { get; private set; }
    public string UserName { get; private set; }
    public string EntityName { get; private set; }
    public Guid RecordId { get; private set; }

    /// <summary>The record's No. or Code (or entry no.) as people know it.</summary>
    public string PrimaryKey { get; private set; }

    public string FieldName { get; private set; }
    public ChangeLogEntryType TypeOfChange { get; private set; }
    public string OldValue { get; private set; }
    public string NewValue { get; private set; }

    protected ChangeLogEntry() { }

    public ChangeLogEntry(
        Guid id,
        DateTime changedAt,
        Guid? userId,
        string userName,
        string entityName,
        Guid recordId,
        string primaryKey,
        string fieldName,
        ChangeLogEntryType typeOfChange,
        string oldValue,
        string newValue
    )
        : base(id)
    {
        ChangedAt = changedAt;
        UserId = userId;
        UserName = Truncate(userName, 256);
        EntityName = Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxNameLength);
        RecordId = recordId;
        PrimaryKey = Truncate(primaryKey, MaxPrimaryKeyLength);
        FieldName = Check.NotNullOrWhiteSpace(fieldName, nameof(fieldName), MaxFieldNameLength);
        TypeOfChange = typeOfChange;
        OldValue = Truncate(oldValue, MaxValueLength);
        NewValue = Truncate(newValue, MaxValueLength);
    }

    private static string Truncate(string value, int length) =>
        value == null ? null : value.Length > length ? value[..length] : value;
}

/// <summary>What one company logs, as the save pipeline reads it.</summary>
public sealed class ChangeLogConfiguration
{
    public static readonly ChangeLogConfiguration Off = new(false, new Dictionary<string, ChangeLogTableSettings>());

    public ChangeLogConfiguration(bool activated, IReadOnlyDictionary<string, ChangeLogTableSettings> tables)
    {
        Activated = activated;
        Tables = tables;
    }

    public bool Activated { get; }

    /// <summary>By entity name.</summary>
    public IReadOnlyDictionary<string, ChangeLogTableSettings> Tables { get; }

    public bool LogsAnything => Activated && Tables.Count > 0;
}

public sealed record ChangeLogTableSettings(
    ChangeLogFieldFilterType LogInsertion,
    ChangeLogFieldFilterType LogModification,
    ChangeLogFieldFilterType LogDeletion,
    IReadOnlySet<string> TrackedFields
)
{
    public ChangeLogFieldFilterType FilterFor(ChangeLogEntryType type) => type switch
    {
        ChangeLogEntryType.Insertion => LogInsertion,
        ChangeLogEntryType.Modification => LogModification,
        _ => LogDeletion,
    };

    public bool Logs(ChangeLogEntryType type, string fieldName) => FilterFor(type) switch
    {
        ChangeLogFieldFilterType.AllFields => true,
        ChangeLogFieldFilterType.SomeFields => TrackedFields.Contains(fieldName),
        _ => false,
    };

    public static ChangeLogTableSettings From(ChangeLogTable table) =>
        new(table.LogInsertion, table.LogModification, table.LogDeletion, table.GetTrackedFields().ToHashSet(StringComparer.Ordinal));
}

/// <summary>
/// Every save asks what its company logs, so the answer is kept in memory for a minute per company.
/// Changing the setup clears it at once on this instance; other instances pick it up within the minute.
/// </summary>
public class ChangeLogConfigurationCache : ISingletonDependency
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<(Guid? TenantId, Guid CompanyId), (DateTime LoadedAt, ChangeLogConfiguration Value)> _entries = new();

    public bool TryGet(Guid? tenantId, Guid companyId, out ChangeLogConfiguration configuration)
    {
        if (_entries.TryGetValue((tenantId, companyId), out var cached) && DateTime.UtcNow - cached.LoadedAt < Lifetime)
        {
            configuration = cached.Value;
            return true;
        }

        configuration = null;
        return false;
    }

    public void Set(Guid? tenantId, Guid companyId, ChangeLogConfiguration configuration) =>
        _entries[(tenantId, companyId)] = (DateTime.UtcNow, configuration);

    public void Invalidate(Guid? tenantId, Guid companyId) => _entries.TryRemove((tenantId, companyId), out _);
}
