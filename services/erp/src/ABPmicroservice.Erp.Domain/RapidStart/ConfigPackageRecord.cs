using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// A record staged in a package, waiting to be validated and applied. Business Central holds these
/// in "Config. Package Record" with one "Config. Package Data" row per field; here the field
/// values travel together as one JSON object of text, which is also how they arrived.
/// </summary>
public class ConfigPackageRecord : CompanyBasicEntity
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Guid ConfigPackageId { get; private set; }

    public Guid ConfigPackageTableId { get; private set; }

    /// <summary>Position within the table, starting at 1: the row a user finds it on in the file.</summary>
    public int RecordNo { get; private set; }

    /// <summary>Field name to value, as JSON. Values are text until they are applied.</summary>
    public string Values { get; private set; }

    /// <summary>The last validation or apply found an error in this record.</summary>
    public bool Invalid { get; private set; }

    protected ConfigPackageRecord() { }

    public ConfigPackageRecord(Guid id, Guid configPackageId, Guid configPackageTableId, int recordNo, IReadOnlyDictionary<string, string> values)
        : base(id)
    {
        ConfigPackageId = configPackageId;
        ConfigPackageTableId = configPackageTableId;
        RecordNo = recordNo;
        SetValues(values);
    }

    public Dictionary<string, string> GetValues()
    {
        var values = Values.IsNullOrWhiteSpace()
            ? null
            : JsonSerializer.Deserialize<Dictionary<string, string>>(Values, JsonOptions);

        return new Dictionary<string, string>(values ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
    }

    public void SetValues(IReadOnlyDictionary<string, string> values)
    {
        var cleaned = (values ?? new Dictionary<string, string>())
            .Where(p => !p.Key.IsNullOrWhiteSpace())
            .ToDictionary(p => p.Key.Trim(), p => Truncate(p.Value), StringComparer.OrdinalIgnoreCase);

        Values = JsonSerializer.Serialize(cleaned);
    }

    public void SetValue(string fieldName, string value)
    {
        var values = GetValues();
        values[Check.NotNullOrWhiteSpace(fieldName, nameof(fieldName))] = value;
        SetValues(values);
    }

    public void MarkInvalid(bool invalid) => Invalid = invalid;

    private static string Truncate(string value)
    {
        return value == null || value.Length <= ErpDomainConsts.MaxConfigValueLength
            ? value
            : value[..ErpDomainConsts.MaxConfigValueLength];
    }
}

/// <summary>
/// Why a staged record could not be applied. Mirrors BC table 8617 "Config. Package Error": one
/// row per field that failed, so the error can be shown next to the value that caused it.
/// </summary>
public class ConfigPackageError : CompanyBasicEntity
{
    public Guid ConfigPackageId { get; private set; }

    public Guid ConfigPackageTableId { get; private set; }

    public Guid ConfigPackageRecordId { get; private set; }

    public int RecordNo { get; private set; }

    /// <summary>The field at fault, or null when the record as a whole failed.</summary>
    public string FieldName { get; private set; }

    public string ErrorText { get; private set; }

    protected ConfigPackageError() { }

    public ConfigPackageError(
        Guid id,
        Guid configPackageId,
        Guid configPackageTableId,
        Guid configPackageRecordId,
        int recordNo,
        string fieldName,
        string errorText
    )
        : base(id)
    {
        ConfigPackageId = configPackageId;
        ConfigPackageTableId = configPackageTableId;
        ConfigPackageRecordId = configPackageRecordId;
        RecordNo = recordNo;
        FieldName = fieldName;

        errorText ??= string.Empty;
        ErrorText = errorText.Length <= ErpDomainConsts.MaxConfigErrorLength
            ? errorText
            : errorText[..ErpDomainConsts.MaxConfigErrorLength];
    }
}
