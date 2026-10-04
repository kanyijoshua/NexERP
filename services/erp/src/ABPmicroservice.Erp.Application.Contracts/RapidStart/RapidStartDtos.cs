using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ABPmicroservice.Erp.Exporting;
using Volo.Abp.Application.Dtos;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>A table a package or the import wizard can write to.</summary>
public class ConfigTableInfoDto
{
    public string EntityName { get; set; }

    public string DisplayName { get; set; }

    /// <summary>Functional area, used to group tables on the worksheet and in pickers.</summary>
    public string Area { get; set; }

    public List<string> KeyFields { get; set; } = new();

    /// <summary>For a line table, its header table.</summary>
    public string ParentEntity { get; set; }

    /// <summary>Tables this one refers to, which have to be set up first.</summary>
    public List<string> RelatedTables { get; set; } = new();

    /// <summary>Whether the caller holds the permissions to write to the table.</summary>
    public bool CanWrite { get; set; }
}

public class ConfigFieldInfoDto
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    /// <summary>"string", "number", "date", "boolean", "guid" or "enum", as the export dialog describes fields.</summary>
    public string DataType { get; set; }

    public List<string> EnumValues { get; set; }

    public bool IsKey { get; set; }

    public bool IsRequired { get; set; }

    public int? MaxLength { get; set; }

    /// <summary>The table the value must exist in, if any. A reference field takes that table's key.</summary>
    public string RelatedTable { get; set; }
}

public class ConfigPackageDto : EntityDto<Guid>
{
    public string Code { get; set; }

    public string PackageName { get; set; }

    public string ProductVersion { get; set; }

    public DateTime? LastImportedTime { get; set; }

    public DateTime? LastAppliedTime { get; set; }

    public int NoOfTables { get; set; }

    public int NoOfRecords { get; set; }

    public int NoOfErrors { get; set; }
}

public class ConfigPackageDetailDto : ConfigPackageDto
{
    public List<ConfigPackageTableDto> Tables { get; set; } = new();
}

public class ConfigPackageTableDto : EntityDto<Guid>
{
    public string EntityName { get; set; }

    public string DisplayName { get; set; }

    public int ProcessingOrder { get; set; }

    public bool DeleteRecordsBeforeProcessing { get; set; }

    public string DataTemplateCode { get; set; }

    public List<EntityFilterDto> Filters { get; set; } = new();

    public int NoOfRecords { get; set; }

    public int NoOfErrors { get; set; }

    public List<ConfigPackageFieldDto> Fields { get; set; } = new();
}

public class ConfigPackageFieldDto : EntityDto<Guid>
{
    public string FieldName { get; set; }

    public string DisplayName { get; set; }

    public string DataType { get; set; }

    public bool IncludeField { get; set; }

    public bool ValidateField { get; set; }

    public int ProcessingOrder { get; set; }

    public bool PrimaryKey { get; set; }

    public string RelatedTable { get; set; }

    public List<ConfigFieldMappingDto> Mappings { get; set; } = new();
}

public class ConfigFieldMappingDto
{
    [StringLength(ErpDomainConsts.MaxConfigValueLength)]
    public string OldValue { get; set; }

    [StringLength(ErpDomainConsts.MaxConfigValueLength)]
    public string NewValue { get; set; }
}

public class GetConfigPackagesInput
{
    public string Filter { get; set; }
}

public class CreateConfigPackageDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxPackageCodeLength)]
    public string Code { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string PackageName { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ProductVersion { get; set; }
}

public class UpdateConfigPackageDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string PackageName { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string ProductVersion { get; set; }
}

public class IncludeConfigTablesInput
{
    [Required]
    [MinLength(1)]
    public List<string> EntityNames { get; set; } = new();

    /// <summary>Also add the tables these refer to and their line tables: Get Related Tables.</summary>
    public bool IncludeRelatedTables { get; set; }
}

public class ConfigPackageTableInput
{
    public Guid TableId { get; set; }
}

/// <summary>Which tables of a package to act on. Empty means all of them.</summary>
public class ConfigPackageTablesInput
{
    public List<Guid> TableIds { get; set; } = new();
}

public class UpdateConfigPackageTableDto
{
    public Guid TableId { get; set; }

    public int ProcessingOrder { get; set; }

    public bool DeleteRecordsBeforeProcessing { get; set; }

    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string DataTemplateCode { get; set; }

    public List<EntityFilterDto> Filters { get; set; } = new();

    /// <summary>Fields to change. A field left out keeps its settings.</summary>
    public List<UpdateConfigPackageFieldDto> Fields { get; set; } = new();
}

public class UpdateConfigPackageFieldDto
{
    [Required]
    public string FieldName { get; set; }

    public bool IncludeField { get; set; }

    public bool ValidateField { get; set; }

    public int ProcessingOrder { get; set; }

    public List<ConfigFieldMappingDto> Mappings { get; set; } = new();
}

public class ConfigTableRunResultDto
{
    public Guid TableId { get; set; }

    public string EntityName { get; set; }

    public int Inserted { get; set; }

    public int Modified { get; set; }

    public int Deleted { get; set; }

    /// <summary>Records that failed.</summary>
    public int Errors { get; set; }
}

public class ConfigPackageRunResultDto
{
    public List<ConfigTableRunResultDto> Tables { get; set; } = new();

    public int Inserted { get; set; }

    public int Modified { get; set; }

    public int Errors { get; set; }
}

public class ExportConfigPackageInput
{
    public ConfigPackageFileFormat Format { get; set; }
}

/// <summary>
/// A file sent as base64 inside the JSON body. The files involved are small (the service refuses
/// anything over <see cref="ErpDomainConsts.MaxImportFileBytes"/>), and one JSON call keeps the
/// generated client simple where a multipart form would not.
/// </summary>
public class UploadFileInput
{
    [Required]
    [StringLength(260)]
    public string FileName { get; set; }

    [Required]
    public string ContentBase64 { get; set; }
}

public class ImportConfigDataResultDto
{
    public int NoOfRecords { get; set; }
}

public class GetConfigPackageRecordsInput : PagedResultRequestDto
{
    public Guid PackageId { get; set; }

    public Guid TableId { get; set; }

    /// <summary>Only records that failed validation or apply.</summary>
    public bool ErrorsOnly { get; set; }
}

public class ConfigPackageRecordDto : EntityDto<Guid>
{
    public int RecordNo { get; set; }

    public Dictionary<string, string> Values { get; set; } = new();

    public bool Invalid { get; set; }

    public List<ConfigPackageErrorDto> Errors { get; set; } = new();
}

public class ConfigPackageErrorDto
{
    public Guid TableId { get; set; }

    public string EntityName { get; set; }

    public Guid RecordId { get; set; }

    public int RecordNo { get; set; }

    public string FieldName { get; set; }

    public string ErrorText { get; set; }
}

public class UpdateConfigPackageRecordDto
{
    public Dictionary<string, string> Values { get; set; } = new();
}

public class ConfigTemplateDto : EntityDto<Guid>
{
    public string Code { get; set; }

    public string Description { get; set; }

    public string EntityName { get; set; }

    public bool Enabled { get; set; }

    public List<ConfigTemplateLineDto> Lines { get; set; } = new();
}

public class ConfigTemplateLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string FieldName { get; set; }

    [StringLength(ErpDomainConsts.MaxConfigValueLength)]
    public string DefaultValue { get; set; }

    public bool Mandatory { get; set; }
}

public class CreateUpdateConfigTemplateDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string EntityName { get; set; }

    public bool Enabled { get; set; } = true;

    public List<ConfigTemplateLineDto> Lines { get; set; } = new();
}

public class GetConfigTemplatesInput
{
    /// <summary>Blank lists the templates of every table.</summary>
    public string EntityName { get; set; }
}

public class ConfigLineDto : EntityDto<Guid>
{
    public ConfigLineType LineType { get; set; }

    public string Name { get; set; }

    public string EntityName { get; set; }

    public string PackageCode { get; set; }

    public ConfigLineStatus Status { get; set; }

    public string ResponsibleUserName { get; set; }

    public string Comments { get; set; }

    public int SortOrder { get; set; }

    /// <summary>Records the table holds now, for a table line the caller may read.</summary>
    public long? NoOfRecords { get; set; }
}

public class CreateConfigLineDto
{
    public ConfigLineType LineType { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string EntityName { get; set; }

    [StringLength(ErpDomainConsts.MaxPackageCodeLength)]
    public string PackageCode { get; set; }

    public ConfigLineStatus Status { get; set; }

    [StringLength(ErpDomainConsts.MaxUserNameLength)]
    public string ResponsibleUserName { get; set; }

    [StringLength(ErpDomainConsts.MaxCommentLength)]
    public string Comments { get; set; }
}

public class UpdateConfigLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(ErpDomainConsts.MaxPackageCodeLength)]
    public string PackageCode { get; set; }

    public ConfigLineStatus Status { get; set; }

    [StringLength(ErpDomainConsts.MaxUserNameLength)]
    public string ResponsibleUserName { get; set; }

    [StringLength(ErpDomainConsts.MaxCommentLength)]
    public string Comments { get; set; }
}

public class MoveConfigLineInput
{
    /// <summary>True moves the line one place up, false one place down.</summary>
    public bool Up { get; set; }
}

/// <summary>A file for the import wizard, with how to read it.</summary>
public class ImportFileInput : UploadFileInput
{
    /// <summary>Which tab of a workbook to read. Blank takes the first.</summary>
    public string SheetName { get; set; }

    /// <summary>"," ";" or a tab for a CSV file. Blank detects it from the header line.</summary>
    [StringLength(1)]
    public string Separator { get; set; }

    /// <summary>Whether the first row names the columns rather than holding a record.</summary>
    public bool HasHeaders { get; set; } = true;

    /// <summary>With it, columns are matched to this table's fields; without it, only the file is read.</summary>
    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string EntityName { get; set; }
}

/// <summary>One column of the file and the field it goes to. A blank field leaves the column out.</summary>
public class ImportColumnDto
{
    public int Index { get; set; }

    public string Header { get; set; }

    public string FieldName { get; set; }
}

public class ImportFilePreviewDto
{
    public List<string> Sheets { get; set; } = new();

    public string SheetName { get; set; }

    public List<ImportColumnDto> Columns { get; set; } = new();

    /// <summary>The first rows under the header, as they will be read.</summary>
    public List<List<string>> SampleRows { get; set; } = new();

    public int TotalRows { get; set; }
}

public class RunImportInput : ImportFileInput
{
    public List<ImportColumnDto> Columns { get; set; } = new();

    /// <summary>A configuration template whose values fill what a new record leaves blank.</summary>
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string DataTemplateCode { get; set; }
}

public class ImportErrorDto
{
    /// <summary>The row in the file, counting the header row, as a spreadsheet numbers it.</summary>
    public int RowNo { get; set; }

    public string FieldName { get; set; }

    public string Message { get; set; }
}

public class ImportResultDto
{
    /// <summary>Nothing was written: the run was a test, or it found errors.</summary>
    public bool DryRun { get; set; }

    public bool Succeeded { get; set; }

    public int TotalRows { get; set; }

    public int Inserted { get; set; }

    public int Modified { get; set; }

    public List<ImportErrorDto> Errors { get; set; } = new();
}
