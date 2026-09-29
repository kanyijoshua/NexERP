namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// File a configuration package travels in. Business Central writes a package file of its own
/// (definition and data together) and an Excel workbook with one sheet per table; this follows
/// both, with JSON in place of BC's compressed XML.
/// </summary>
public enum ConfigPackageFileFormat
{
    /// <summary>Definition and data: what another company or system needs to recreate the package.</summary>
    Json = 0,

    /// <summary>Data only, one sheet per table, for editing in Excel and importing back.</summary>
    Xlsx = 1,
}

/// <summary>What a configuration worksheet line is. Mirrors BC table 8622 "Config. Line", field "Line Type".</summary>
public enum ConfigLineType
{
    /// <summary>A functional area such as Finance: a heading.</summary>
    Area = 0,

    /// <summary>A group of tables within an area: a subheading.</summary>
    Group = 1,

    /// <summary>A table to set up.</summary>
    Table = 2,
}

/// <summary>How far set-up of a worksheet line has got. Mirrors the Status option of BC's Config. Line.</summary>
public enum ConfigLineStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Ignored = 3,
    Blocked = 4,
}
