namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// File format of a data export. Business Central writes Excel and CSV from its
/// configuration packages; Odoo's export dialog offers CSV and XLSX.
/// </summary>
public enum ExportFormat
{
    Csv = 0,
    Xlsx = 1,
    Json = 2,
}

/// <summary>How one filter line narrows an export or an integration query.</summary>
public enum EntityFilterOperator
{
    Equals = 0,
    NotEquals = 1,
    Contains = 2,
    StartsWith = 3,
    GreaterThan = 4,
    GreaterOrEqual = 5,
    LessThan = 6,
    LessOrEqual = 7,
}
