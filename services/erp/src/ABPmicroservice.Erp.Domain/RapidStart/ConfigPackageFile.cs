using System.Collections.Generic;
using ABPmicroservice.Erp.Exporting;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// A configuration package as a file: its definition, the data templates its tables use, and the
/// records of every table. Business Central's .rapidstart file carries the same three things as
/// compressed XML; this is the JSON equivalent, readable and diffable.
/// </summary>
public class ConfigPackageFile
{
    public const string FormatName = "abp-erp-config-package";

    public string Format { get; set; } = FormatName;

    public int Version { get; set; } = 1;

    public string Code { get; set; }

    public string PackageName { get; set; }

    public string ProductVersion { get; set; }

    public List<ConfigPackageFileTemplate> Templates { get; set; } = [];

    public List<ConfigPackageFileTable> Tables { get; set; } = [];
}

public class ConfigPackageFileTable
{
    public string EntityName { get; set; }

    public int ProcessingOrder { get; set; }

    public bool DeleteRecordsBeforeProcessing { get; set; }

    public string DataTemplateCode { get; set; }

    public List<EntityFilter> Filters { get; set; } = [];

    public List<ConfigPackageFileField> Fields { get; set; } = [];

    /// <summary>Field name to value, every value as text.</summary>
    public List<Dictionary<string, string>> Records { get; set; } = [];
}

public class ConfigPackageFileField
{
    public string Name { get; set; }

    public bool Include { get; set; } = true;

    public bool Validate { get; set; } = true;

    public int ProcessingOrder { get; set; }

    public List<ConfigFieldMapping> Mappings { get; set; } = [];
}

public class ConfigPackageFileTemplate
{
    public string Code { get; set; }

    public string Description { get; set; }

    public string EntityName { get; set; }

    public bool Enabled { get; set; } = true;

    public List<ConfigPackageFileTemplateLine> Lines { get; set; } = [];
}

public class ConfigPackageFileTemplateLine
{
    public string FieldName { get; set; }

    public string DefaultValue { get; set; }

    public bool Mandatory { get; set; }
}
