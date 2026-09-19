using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// Configuration Package. Mirrors Business Central table 8623 "Config. Package".
/// Seeding and data migration package entity.
/// </summary>
public class ConfigPackage : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string Code { get; private set; }
    public string PackageName { get; private set; }
    public string ProductVersion { get; private set; }
    public Collection<ConfigPackageTable> Tables { get; private set; }

    protected ConfigPackage()
    {
        Tables = new Collection<ConfigPackageTable>();
    }

    public ConfigPackage(Guid id, string code, string packageName, string productVersion = "24.0")
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxPackageCodeLength);
        PackageName = Check.NotNullOrWhiteSpace(packageName, nameof(packageName), ErpDomainConsts.MaxNameLength);
        ProductVersion = productVersion;
        Tables = new Collection<ConfigPackageTable>();
    }
}

/// <summary>
/// Configuration Package Table. Mirrors Business Central table 8613 "Config. Package Table".
/// </summary>
public class ConfigPackageTable : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ConfigPackageId { get; private set; }
    public int TableId { get; private set; }
    public string TableName { get; private set; }
    public int NoOfRecords { get; internal set; }
    public Collection<ConfigPackageField> Fields { get; private set; }

    protected ConfigPackageTable()
    {
        Fields = new Collection<ConfigPackageField>();
    }

    public ConfigPackageTable(Guid id, Guid configPackageId, int tableId, string tableName)
        : base(id)
    {
        ConfigPackageId = configPackageId;
        TableId = tableId;
        TableName = Check.NotNullOrWhiteSpace(tableName, nameof(tableName));
        NoOfRecords = 0;
        Fields = new Collection<ConfigPackageField>();
    }
}

/// <summary>
/// Configuration Package Field. Mirrors Business Central table 8616 "Config. Package Field".
/// </summary>
public class ConfigPackageField : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid ConfigPackageTableId { get; private set; }
    public int FieldId { get; private set; }
    public string FieldName { get; private set; }
    public bool PrimaryKey { get; private set; }
    public bool IncludeField { get; private set; }

    protected ConfigPackageField() { }

    public ConfigPackageField(Guid id, Guid configPackageTableId, int fieldId, string fieldName, bool primaryKey = false)
        : base(id)
    {
        ConfigPackageTableId = configPackageTableId;
        FieldId = fieldId;
        FieldName = Check.NotNullOrWhiteSpace(fieldName, nameof(fieldName));
        PrimaryKey = primaryKey;
        IncludeField = true;
    }
}
