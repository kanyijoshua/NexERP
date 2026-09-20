using System;
using System.Collections.Generic;

namespace ABPmicroservice.Erp.Reporting;

public class ReportColumnDefinition
{
    public ReportColumnDefinition(string key, string header, ReportColumnKind kind = ReportColumnKind.Number)
    {
        Key = key;
        Header = header;
        Kind = kind;
    }

    /// <summary>Key of the value in <see cref="ReportRow.Values"/>.</summary>
    public string Key { get; }

    /// <summary>Column caption. Already localized or taken from the user's own layout.</summary>
    public string Header { get; }

    public ReportColumnKind Kind { get; }
}

public class ReportRow
{
    public Dictionary<string, object> Values { get; } = new();

    public bool Bold { get; set; }

    public bool Italic { get; set; }

    /// <summary>Nesting level, so a schedule can be read as a tree.</summary>
    public int Indentation { get; set; }

    /// <summary>
    /// Accounts behind the row, in the Totaling syntax ("1000..1999"). The report viewer passes
    /// it back to the G/L detail report, which is how BC's drill-down works.
    /// </summary>
    public string DrillDownFilter { get; set; }
}

/// <summary>
/// One shape for every report: a set of columns and the rows under them. Keeping all reports in
/// this shape lets one Angular viewer and one exporter serve all of them.
/// </summary>
public class ReportResult
{
    public string Title { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public List<ReportColumnDefinition> Columns { get; } = new();

    public List<ReportRow> Rows { get; } = new();

    public ReportRow AddRow()
    {
        var row = new ReportRow();
        Rows.Add(row);
        return row;
    }
}
