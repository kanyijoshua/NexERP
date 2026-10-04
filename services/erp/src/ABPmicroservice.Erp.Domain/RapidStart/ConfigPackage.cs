using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Exporting;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// Configuration package: a named set of
/// tables, each with the fields and filters to carry, used to move setup and master data into a
/// company — from a file, from Excel, or from another company.
/// </summary>
public class ConfigPackage : CompanyAggregateRoot
{
    public string Code { get; private set; }

    public string PackageName { get; private set; }

    /// <summary>Version of the system the package was made for; informational.</summary>
    public string ProductVersion { get; private set; }

    public DateTime? LastImportedTime { get; private set; }

    public DateTime? LastAppliedTime { get; private set; }

    public Collection<ConfigPackageTable> Tables { get; private set; }

    protected ConfigPackage()
    {
        Tables = new Collection<ConfigPackageTable>();
    }

    public ConfigPackage(Guid id, string code, string packageName, string productVersion = null)
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxPackageCodeLength).Trim().ToUpperInvariant();
        Tables = new Collection<ConfigPackageTable>();
        Update(packageName, productVersion);
    }

    public void Update(string packageName, string productVersion)
    {
        PackageName = Check.NotNullOrWhiteSpace(packageName, nameof(packageName), ErpDomainConsts.MaxNameLength);
        ProductVersion = Check.Length(productVersion, nameof(productVersion), ErpDomainConsts.MaxCodeLength);
    }

    /// <summary>
    /// Adds a table with every field it can carry, key fields marked as such:
    /// all fields included until someone excludes some.
    /// </summary>
    public ConfigPackageTable AddTable(Guid tableId, ConfigTableProfile profile, int processingOrder, Func<Guid> newId)
    {
        if (FindTable(profile.Name) != null)
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.TableAlreadyInPackage)
                .WithData("entityName", profile.Name)
                .WithData("packageCode", Code);
        }

        var table = new ConfigPackageTable(tableId, Id, profile.Name, processingOrder);

        var order = 0;
        foreach (var field in profile.ImportableFields)
        {
            table.Fields.Add(new ConfigPackageField(newId(), tableId, field.Name, ++order, profile.IsKeyField(field.Name)));
        }

        Tables.Add(table);
        return table;
    }

    public void RemoveTable(Guid tableId)
    {
        Tables.Remove(GetTable(tableId));
    }

    public ConfigPackageTable FindTable(string entityName)
    {
        return Tables.FirstOrDefault(t => string.Equals(t.EntityName, entityName, StringComparison.OrdinalIgnoreCase));
    }

    public ConfigPackageTable GetTable(Guid tableId)
    {
        return Tables.FirstOrDefault(t => t.Id == tableId)
            ?? throw new BusinessException(ErpErrorCodes.RapidStart.TableNotInPackage).WithData("packageCode", Code);
    }

    /// <summary>Tables in the order they are applied: processing order, then name for a stable tie-break.</summary>
    public IReadOnlyList<ConfigPackageTable> TablesInProcessingOrder()
    {
        return Tables.OrderBy(t => t.ProcessingOrder).ThenBy(t => t.EntityName, StringComparer.Ordinal).ToList();
    }

    public void MarkImported(DateTime time) => LastImportedTime = time;

    public void MarkApplied(DateTime time) => LastAppliedTime = time;
}

/// <summary>A table in a configuration package.</summary>
public class ConfigPackageTable : Entity<Guid>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Guid ConfigPackageId { get; private set; }

    /// <summary>The table, as the entity registry names it.</summary>
    public string EntityName { get; private set; }

    /// <summary>Tables are applied in ascending order, so related tables can go first.</summary>
    public int ProcessingOrder { get; private set; }

    /// <summary>
    /// Empties the table before the package's records are applied. "Delete Recs Before
    /// Processing": for replacing a list outright rather than merging into it.
    /// </summary>
    public bool DeleteRecordsBeforeProcessing { get; private set; }

    /// <summary>
    /// A configuration template whose values fill in what a new record leaves blank. The "Data
    /// Template" on a package table.
    /// </summary>
    public string DataTemplateCode { get; private set; }

    /// <summary>
    /// Which records to take when the package is filled from the database or exported, stored as
    /// JSON.
    /// </summary>
    public string Filters { get; private set; }

    /// <summary>Records staged for this table.</summary>
    public int NoOfRecords { get; private set; }

    /// <summary>Staged records that failed validation or could not be applied.</summary>
    public int NoOfErrors { get; private set; }

    public Collection<ConfigPackageField> Fields { get; private set; }

    protected ConfigPackageTable()
    {
        Fields = new Collection<ConfigPackageField>();
    }

    internal ConfigPackageTable(Guid id, Guid configPackageId, string entityName, int processingOrder)
        : base(id)
    {
        ConfigPackageId = configPackageId;
        EntityName = Check.NotNullOrWhiteSpace(entityName, nameof(entityName), ErpDomainConsts.MaxEntityNameLength);
        ProcessingOrder = processingOrder;
        Fields = new Collection<ConfigPackageField>();
    }

    public void SetOptions(int processingOrder, bool deleteRecordsBeforeProcessing, string dataTemplateCode)
    {
        ProcessingOrder = processingOrder;
        DeleteRecordsBeforeProcessing = deleteRecordsBeforeProcessing;
        DataTemplateCode = dataTemplateCode.IsNullOrWhiteSpace()
            ? null
            : Check.Length(dataTemplateCode.Trim().ToUpperInvariant(), nameof(dataTemplateCode), ErpDomainConsts.MaxCodeLength);
    }

    public void SetFilters(IEnumerable<EntityFilter> filters)
    {
        var list = (filters ?? Enumerable.Empty<EntityFilter>()).Where(f => !f.Field.IsNullOrWhiteSpace()).ToList();
        Filters = list.Count == 0
            ? null
            : Check.Length(JsonSerializer.Serialize(list), nameof(Filters), ErpDomainConsts.MaxConfigSettingsLength);
    }

    public List<EntityFilter> GetFilters()
    {
        return Filters.IsNullOrWhiteSpace()
            ? []
            : JsonSerializer.Deserialize<List<EntityFilter>>(Filters, JsonOptions) ?? [];
    }

    public void SetCounts(int noOfRecords, int noOfErrors)
    {
        NoOfRecords = noOfRecords;
        NoOfErrors = noOfErrors;
    }

    public ConfigPackageField FindField(string fieldName)
    {
        return Fields.FirstOrDefault(f => string.Equals(f.FieldName, fieldName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Fields the package carries, in processing order.</summary>
    public IReadOnlyList<ConfigPackageField> IncludedFields()
    {
        return Fields.Where(f => f.IncludeField).OrderBy(f => f.ProcessingOrder).ThenBy(f => f.FieldName, StringComparer.Ordinal).ToList();
    }
}

/// <summary>A field of a package table.</summary>
public class ConfigPackageField : Entity<Guid>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Guid ConfigPackageTableId { get; private set; }

    public string FieldName { get; private set; }

    /// <summary>Whether the field is exported, imported and applied at all.</summary>
    public bool IncludeField { get; private set; }

    /// <summary>
    /// Whether the value is checked against the field's relation and length before it is written.
    /// "Validate Field": off, the value goes in as it stands.
    /// </summary>
    public bool ValidateField { get; private set; }

    public int ProcessingOrder { get; private set; }

    /// <summary>Part of the table's key. A key field is always included and always validated.</summary>
    public bool PrimaryKey { get; private set; }

    /// <summary>
    /// Values to translate on the way in, stored as JSON: the "Config. Field Mapping", which turns
    /// the codes of an old system into this one's.
    /// </summary>
    public string Mappings { get; private set; }

    protected ConfigPackageField() { }

    internal ConfigPackageField(Guid id, Guid configPackageTableId, string fieldName, int processingOrder, bool primaryKey)
        : base(id)
    {
        ConfigPackageTableId = configPackageTableId;
        FieldName = Check.NotNullOrWhiteSpace(fieldName, nameof(fieldName), ErpDomainConsts.MaxEntityNameLength);
        ProcessingOrder = processingOrder;
        PrimaryKey = primaryKey;
        IncludeField = true;
        ValidateField = true;
    }

    public void Set(bool includeField, bool validateField, int processingOrder)
    {
        IncludeField = PrimaryKey || includeField;
        ValidateField = PrimaryKey || validateField;
        ProcessingOrder = processingOrder;
    }

    public void SetMappings(IEnumerable<ConfigFieldMapping> mappings)
    {
        var list = (mappings ?? Enumerable.Empty<ConfigFieldMapping>())
            .Where(m => !m.OldValue.IsNullOrEmpty())
            .GroupBy(m => m.OldValue, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last())
            .ToList();

        Mappings = list.Count == 0
            ? null
            : Check.Length(JsonSerializer.Serialize(list), nameof(Mappings), ErpDomainConsts.MaxConfigSettingsLength);
    }

    public List<ConfigFieldMapping> GetMappings()
    {
        return Mappings.IsNullOrWhiteSpace()
            ? []
            : JsonSerializer.Deserialize<List<ConfigFieldMapping>>(Mappings, JsonOptions) ?? [];
    }
}

/// <summary>One translation of a field value: whatever reads <see cref="OldValue"/> becomes <see cref="NewValue"/>.</summary>
public class ConfigFieldMapping
{
    public string OldValue { get; set; }

    public string NewValue { get; set; }
}
