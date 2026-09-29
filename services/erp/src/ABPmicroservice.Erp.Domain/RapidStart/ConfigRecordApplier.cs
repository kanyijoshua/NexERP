using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>One record to apply: its row number in the source and its values as text.</summary>
public sealed class ConfigRecordInput
{
    public ConfigRecordInput(int recordNo, IReadOnlyDictionary<string, string> values, Guid? recordId = null)
    {
        RecordNo = recordNo;
        Values = new Dictionary<string, string>(values ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
        RecordId = recordId;
    }

    public int RecordNo { get; }

    public IReadOnlyDictionary<string, string> Values { get; }

    /// <summary>The staged package record this came from, when it came from one.</summary>
    public Guid? RecordId { get; }
}

/// <summary>What went wrong with one field of one record.</summary>
public sealed class ConfigRecordIssue
{
    public ConfigRecordIssue(ConfigRecordInput record, string fieldName, string message)
    {
        RecordNo = record.RecordNo;
        RecordId = record.RecordId;
        FieldName = fieldName;
        Message = message;
    }

    public int RecordNo { get; }

    public Guid? RecordId { get; }

    public string FieldName { get; }

    public string Message { get; }
}

/// <summary>How one table's records are to be applied.</summary>
public sealed class ConfigApplyRequest
{
    public ConfigApplyRequest(ConfigTableProfile profile, IEnumerable<string> fields)
    {
        Profile = profile;
        Fields = fields.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public ConfigTableProfile Profile { get; }

    /// <summary>Fields the records carry. Only these are written to an existing record.</summary>
    public IReadOnlyList<string> Fields { get; }

    /// <summary>Fields whose relation and length are checked. Null checks every field.</summary>
    public ISet<string> ValidatedFields { get; init; }

    public IReadOnlyDictionary<string, List<ConfigFieldMapping>> Mappings { get; init; }

    /// <summary>Defaults for what a new record leaves blank.</summary>
    public ConfigTemplate Template { get; init; }

    /// <summary>Check everything, write nothing: BC's "Validate Package", Odoo's "Test".</summary>
    public bool DryRun { get; init; }
}

public sealed class ConfigApplyResult
{
    public int Inserted { get; set; }

    public int Modified { get; set; }

    public List<ConfigRecordIssue> Errors { get; } = [];

    public int FailedRecords => Errors.Select(e => e.RecordNo).Distinct().Count();
}

/// <summary>
/// Keys known to exist, by table and key field, so a relation can be checked without a query per
/// value. Shared across the tables of one run, so a customer can name a posting group that the
/// same package only creates a moment earlier — which also holds for a dry run, where nothing is
/// written and the key is merely noted.
/// </summary>
public sealed class ConfigApplyContext
{
    private readonly Dictionary<string, Dictionary<string, Guid>> _keys = new(StringComparer.OrdinalIgnoreCase);

    internal bool TryGet(string entityName, string keyField, out Dictionary<string, Guid> keys)
    {
        return _keys.TryGetValue(entityName + "." + keyField, out keys);
    }

    internal void Set(string entityName, string keyField, Dictionary<string, Guid> keys)
    {
        _keys[entityName + "." + keyField] = keys;
    }

    internal void Add(string entityName, string keyField, string key, Guid id)
    {
        if (key != null && _keys.TryGetValue(entityName + "." + keyField, out var keys))
        {
            keys[key] = id;
        }
    }
}

/// <summary>
/// Validates and writes records into one table: the engine behind applying a configuration package
/// and behind the import wizard.
/// <para>
/// For each record it does what Business Central's RapidStart does when it applies a package:
/// translate values through the field mappings, find the existing record by its primary key,
/// fill a new one from the data template, check every value against its type, length and table
/// relation, and then insert or modify. Errors are collected per field rather than stopping the
/// run; the caller decides whether to write the good records anyway (a package, as in BC) or
/// nothing at all (the import wizard, as in Odoo).
/// </para>
/// </summary>
public class ConfigRecordApplier : DomainService
{
    private readonly ConfigTableRegistry _tables;
    private readonly IStringLocalizer<ErpResource> _localizer;
    private readonly IServiceProvider _serviceProvider;

    public ConfigRecordApplier(ConfigTableRegistry tables, IStringLocalizer<ErpResource> localizer, IServiceProvider serviceProvider)
    {
        _tables = tables;
        _localizer = localizer;
        _serviceProvider = serviceProvider;
    }

    public async Task<ConfigApplyResult> ApplyAsync(
        ConfigApplyRequest request,
        IReadOnlyList<ConfigRecordInput> records,
        ConfigApplyContext context = null
    )
    {
        context ??= new ConfigApplyContext();
        var profile = request.Profile;
        var store = StoreFor(profile);
        var result = new ConfigApplyResult();

        // Keys this run has already written (or, in a dry run, would have), so a repeated key
        // counts as a change to the first rather than a second insert.
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // This table's own keys, so the records it adds can be named by the tables that follow it.
        if (profile.KeyFields.Count == 1)
        {
            await GetKeysAsync(profile.Name, profile.KeyFields[0], context);
        }

        // A reference carried only as its code (ParentCategoryCode) still sets the id beside it.
        var requestFields = request.Fields.ToList();
        foreach (var relation in profile.Relations.Where(r => r.IsIdReference && r.CodeFieldName != null))
        {
            if (requestFields.Contains(relation.CodeFieldName, StringComparer.OrdinalIgnoreCase)
                && !requestFields.Contains(relation.FieldName, StringComparer.OrdinalIgnoreCase))
            {
                requestFields.Add(relation.FieldName);
            }
        }

        foreach (var record in OrderParentsFirst(profile, records))
        {
            var errors = new List<ConfigRecordIssue>();
            var text = Translate(request, record);
            DeriveIdsFromCodes(profile, text);

            var key = await ResolveKeyAsync(request, record, text, context, errors);
            if (errors.Count > 0)
            {
                result.Errors.AddRange(errors);
                continue;
            }

            var keyText = KeyText(profile, key);
            var existing = await store.FindAsync(profile, key);
            var isNew = existing == null && !seenKeys.Contains(keyText);

            var fields = requestFields.ToList();
            if (isNew)
            {
                ApplyTemplate(request, record, text, fields, errors);
            }

            var assignments = new List<(string Field, object Value)>();
            foreach (var fieldName in fields.Where(f => !profile.IsKeyField(f)))
            {
                var field = profile.FindImportableField(fieldName);
                if (field == null)
                {
                    errors.Add(Issue(record, fieldName, "RapidStart:Error:FieldNotImportable", fieldName, profile.Name));
                    continue;
                }

                var value = await ConvertAsync(request, record, field.Name, text.GetValueOrDefault(field.Name), context, errors);
                assignments.Add((field.Name, value));
            }

            if (isNew)
            {
                // A required field the source does not carry at all can only be empty on a new record.
                foreach (var missing in profile.ImportableFields.Where(f => profile.IsRequired(f.Name) && !profile.IsKeyField(f.Name)))
                {
                    if (!fields.Contains(missing.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        errors.Add(Issue(record, missing.Name, "RapidStart:Error:Required", missing.DisplayName));
                    }
                }
            }

            if (errors.Count > 0)
            {
                result.Errors.AddRange(errors);
                continue;
            }

            var id = existing is IEntity<Guid> found ? found.Id : GuidGenerator.Create();

            if (!request.DryRun)
            {
                var entity = existing ?? store.CreateNew(id);

                // The key of an existing record stays as it is: it matched whatever the case.
                if (existing == null)
                {
                    foreach (var (fieldName, value) in key)
                    {
                        ConfigEntityWriter.SetValue(entity, fieldName, value);
                    }
                }

                foreach (var (fieldName, value) in assignments)
                {
                    ConfigEntityWriter.SetValue(entity, fieldName, value);
                }

                if (existing == null)
                {
                    await store.InsertAsync(entity);
                }
                else
                {
                    await store.UpdateAsync(entity);
                }
            }

            if (isNew)
            {
                result.Inserted++;
            }
            else
            {
                result.Modified++;
            }

            seenKeys.Add(keyText);
            RememberKey(profile, key, id, context);
        }

        return result;
    }

    /// <summary>Loads the keys of a table into the context, once per run.</summary>
    public async Task<Dictionary<string, Guid>> GetKeysAsync(string entityName, string keyField, ConfigApplyContext context)
    {
        if (context.TryGet(entityName, keyField, out var keys))
        {
            return keys;
        }

        keys = await StoreFor(_tables.Get(entityName)).LoadKeysAsync(keyField);
        context.Set(entityName, keyField, keys);
        return keys;
    }

    public async Task<int> DeleteAllAsync(ConfigTableProfile profile)
    {
        return await StoreFor(profile).DeleteAllAsync();
    }

    private IConfigEntityStore StoreFor(ConfigTableProfile profile)
    {
        return (IConfigEntityStore)
            _serviceProvider.GetRequiredService(typeof(ConfigEntityStore<>).MakeGenericType(profile.Definition.EntityType));
    }

    /// <summary>Field mappings first, so every later check sees the value as this system knows it.</summary>
    private static Dictionary<string, string> Translate(ConfigApplyRequest request, ConfigRecordInput record)
    {
        var text = new Dictionary<string, string>(record.Values, StringComparer.OrdinalIgnoreCase);

        if (request.Mappings == null)
        {
            return text;
        }

        foreach (var (fieldName, mappings) in request.Mappings)
        {
            if (!text.TryGetValue(fieldName, out var value) || value == null)
            {
                continue;
            }

            var mapping = mappings.FirstOrDefault(m => string.Equals(m.OldValue, value.Trim(), StringComparison.OrdinalIgnoreCase));
            if (mapping != null)
            {
                text[fieldName] = mapping.NewValue;
            }
        }

        return text;
    }

    /// <summary>
    /// A file may carry an item's category code and leave the category id out, since the id means
    /// nothing outside this company. The id is then taken from the code.
    /// </summary>
    private static void DeriveIdsFromCodes(ConfigTableProfile profile, Dictionary<string, string> text)
    {
        foreach (var relation in profile.Relations.Where(r => r.IsIdReference && r.CodeFieldName != null))
        {
            if (text.GetValueOrDefault(relation.FieldName).IsNullOrWhiteSpace() && text.TryGetValue(relation.CodeFieldName, out var code))
            {
                text[relation.FieldName] = code;
            }
        }
    }

    private async Task<Dictionary<string, object>> ResolveKeyAsync(
        ConfigApplyRequest request,
        ConfigRecordInput record,
        Dictionary<string, string> text,
        ConfigApplyContext context,
        List<ConfigRecordIssue> errors
    )
    {
        var key = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var fieldName in request.Profile.KeyFields)
        {
            key[fieldName] = await ConvertAsync(request, record, fieldName, text.GetValueOrDefault(fieldName), context, errors);
        }

        return key;
    }

    private void ApplyTemplate(
        ConfigApplyRequest request,
        ConfigRecordInput record,
        Dictionary<string, string> text,
        List<string> fields,
        List<ConfigRecordIssue> errors
    )
    {
        if (request.Template is not { Enabled: true })
        {
            return;
        }

        foreach (var line in request.Template.Lines)
        {
            if (text.GetValueOrDefault(line.FieldName).IsNullOrWhiteSpace())
            {
                text[line.FieldName] = line.DefaultValue;
            }

            if (!fields.Contains(line.FieldName, StringComparer.OrdinalIgnoreCase))
            {
                fields.Add(line.FieldName);
            }

            if (line.Mandatory && text.GetValueOrDefault(line.FieldName).IsNullOrWhiteSpace())
            {
                errors.Add(Issue(record, line.FieldName, "RapidStart:Error:Required", line.FieldName));
            }
        }
    }

    private async Task<object> ConvertAsync(
        ConfigApplyRequest request,
        ConfigRecordInput record,
        string fieldName,
        string text,
        ConfigApplyContext context,
        List<ConfigRecordIssue> errors
    )
    {
        var profile = request.Profile;
        var field = profile.FindImportableField(fieldName);
        if (field == null)
        {
            errors.Add(Issue(record, fieldName, "RapidStart:Error:FieldNotImportable", fieldName, profile.Name));
            return null;
        }

        var validate = profile.IsKeyField(field.Name) || request.ValidatedFields == null || request.ValidatedFields.Contains(field.Name);
        var relation = profile.FindRelation(field.Name);
        object value;

        if (relation is { IsIdReference: true })
        {
            // The file carries the related record's key; the id is looked up here.
            value = null;
            if (!text.IsNullOrWhiteSpace())
            {
                var keys = await GetKeysAsync(relation.TargetEntity, relation.TargetKeyField, context);
                if (keys.TryGetValue(text.Trim(), out var id))
                {
                    value = id;
                }
                else if (Guid.TryParse(text, out var raw) && keys.ContainsValue(raw))
                {
                    value = raw;
                }
                else
                {
                    errors.Add(RelationIssue(record, profile, field.Name, text, relation));
                    return null;
                }
            }
            else if (!field.IsNullable)
            {
                value = Guid.Empty;
            }
        }
        else if (!ConfigValueConverter.TryConvert(field, text, out value))
        {
            errors.Add(Issue(record, field.Name, "RapidStart:Error:NotValid", text, field.DisplayName));
            return null;
        }

        if (value is string s && profile.IsUpperCase(field.Name))
        {
            value = s.ToUpperInvariant();
        }

        if (profile.IsRequired(field.Name) && IsEmpty(value))
        {
            errors.Add(Issue(record, field.Name, "RapidStart:Error:Required", field.DisplayName));
            return value;
        }

        if (!validate || value is not string textValue)
        {
            return value;
        }

        var maxLength = profile.MaxLength(field.Name);
        if (maxLength.HasValue && textValue.Length > maxLength.Value)
        {
            errors.Add(Issue(record, field.Name, "RapidStart:Error:TooLong", field.DisplayName, maxLength.Value));
            return value;
        }

        if (relation != null && !textValue.IsNullOrWhiteSpace())
        {
            var keys = await GetKeysAsync(relation.TargetEntity, relation.TargetKeyField, context);
            if (!keys.ContainsKey(textValue))
            {
                errors.Add(RelationIssue(record, profile, field.Name, textValue, relation));
            }
        }

        return value;
    }

    /// <summary>
    /// Makes the record's key available to relations checked later in the same run. Relations
    /// always point at a table with a single key field, so only those need remembering.
    /// </summary>
    private static void RememberKey(ConfigTableProfile profile, Dictionary<string, object> key, Guid id, ConfigApplyContext context)
    {
        if (profile.KeyFields.Count == 1)
        {
            context.Add(profile.Name, profile.KeyFields[0], key.GetValueOrDefault(profile.KeyFields[0])?.ToString(), id);
        }
    }

    private static string KeyText(ConfigTableProfile profile, Dictionary<string, object> key)
    {
        return string.Join("\u001f", profile.KeyFields.Select(f => ConfigValueConverter.ToText(key.GetValueOrDefault(f))?.ToUpperInvariant()));
    }

    /// <summary>
    /// Within a table that refers to itself (an item category and its parent), a parent must be in
    /// place before its children. Records are otherwise kept in their original order.
    /// </summary>
    private static IEnumerable<ConfigRecordInput> OrderParentsFirst(ConfigTableProfile profile, IReadOnlyList<ConfigRecordInput> records)
    {
        var selfRelations = profile.Relations.Where(r => string.Equals(r.TargetEntity, profile.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        if (selfRelations.Count == 0 || profile.KeyFields.Count != 1)
        {
            return records;
        }

        var keyField = profile.KeyFields[0];
        var byKey = new Dictionary<string, ConfigRecordInput>(StringComparer.OrdinalIgnoreCase);
        foreach (var record in records)
        {
            var key = record.Values.GetValueOrDefault(keyField)?.Trim();
            if (!key.IsNullOrEmpty())
            {
                byKey.TryAdd(key, record);
            }
        }

        var ordered = new List<ConfigRecordInput>(records.Count);
        var placed = new HashSet<ConfigRecordInput>();

        void Place(ConfigRecordInput record, int depth)
        {
            if (placed.Contains(record))
            {
                return;
            }

            // The depth bound stops a cycle (A is B's parent, B is A's) from recursing forever.
            if (depth < records.Count)
            {
                foreach (var relation in selfRelations)
                {
                    var parentKey = record.Values.GetValueOrDefault(relation.CodeFieldName ?? relation.FieldName)?.Trim();
                    if (!parentKey.IsNullOrEmpty() && byKey.TryGetValue(parentKey, out var parent) && parent != record)
                    {
                        Place(parent, depth + 1);
                    }
                }
            }

            if (placed.Add(record))
            {
                ordered.Add(record);
            }
        }

        foreach (var record in records)
        {
            Place(record, 0);
        }

        return ordered;
    }

    private static bool IsEmpty(object value)
    {
        return value switch
        {
            null => true,
            string s => s.IsNullOrWhiteSpace(),
            Guid g => g == Guid.Empty,
            _ => false,
        };
    }

    private ConfigRecordIssue RelationIssue(ConfigRecordInput record, ConfigTableProfile profile, string fieldName, string value, ConfigTableRelation relation)
    {
        return Issue(
            record,
            fieldName,
            "RapidStart:Error:RelationNotFound",
            fieldName,
            profile.Name,
            value?.Trim(),
            relation.TargetEntity
        );
    }

    private ConfigRecordIssue Issue(ConfigRecordInput record, string fieldName, string messageKey, params object[] arguments)
    {
        return new ConfigRecordIssue(record, fieldName, _localizer[messageKey, arguments]);
    }
}
