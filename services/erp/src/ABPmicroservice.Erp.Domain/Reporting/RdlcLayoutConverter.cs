using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Volo.Abp;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>How one column of the converted layout came about.</summary>
public sealed class RdlcColumnMatch
{
    /// <summary>The caption the layout prints above the column.</summary>
    public string Caption { get; init; }

    /// <summary>The dataset field the RDLC column showed; null for a column the RDLC did not have.</summary>
    public string RdlcField { get; init; }

    /// <summary>The report column that now fills it; null for an RDLC column nothing in the report matches.</summary>
    public string ReportColumnKey { get; init; }
}

/// <summary>An RDLC layout as a report layout of this system, with an account of what carried over.</summary>
public sealed class RdlcConversion
{
    /// <summary>The HTML layout, in the placeholder language of <see cref="ReportTemplate"/>.</summary>
    public string Template { get; init; }

    public IReadOnlyList<RdlcColumnMatch> Columns { get; init; }

    /// <summary>RDLC columns matched to a column of the report.</summary>
    public int MatchedColumns => Columns.Count(c => c.RdlcField != null && c.ReportColumnKey != null);

    /// <summary>What could not be carried over, in sentences a user can act on.</summary>
    public IReadOnlyList<string> Notes { get; init; }
}

/// <summary>
/// Converts an RDLC report layout into a layout of this system.
/// <para>
/// An RDLC file is bound to the dataset of the AL report it was drawn for, and a report here has a
/// dataset of its own, so a layout cannot be carried over field for field. What carries over is
/// what the layout looks like: the page size and margins, the heading, the fonts and colours, and
/// the columns of its main table in their order and widths. Each of those columns is matched to a
/// column of the target report by its caption and field name; a column of the report the RDLC did
/// not show is added at the end, and an RDLC column the report has nothing for is left out and
/// reported. Expressions, images, sub-reports and charts are not converted.
/// </para>
/// <para>
/// The file is treated as untrusted: it is parsed with DTDs prohibited, every piece of text is
/// escaped, and a style value is used only if it has the shape of a colour, a length or a font.
/// </para>
/// </summary>
public static partial class RdlcLayoutConverter
{
    /// <summary>Largest RDLC file accepted; the largest hand-drawn layouts are under a megabyte.</summary>
    public const int MaxRdlcBytes = 5 * 1024 * 1024;

    private const double MatchThreshold = 0.6;

    /// <summary>Words that say nothing about what a column holds.</summary>
    private static readonly HashSet<string> Noise = new(StringComparer.Ordinal)
    {
        "caption", "lbl", "value", "format", "abs", "the", "of", "at", "as", "in", "rec", "check", "uppercase", "lcy", "fields",
        "vendor", "customer", "member", "first", "sum", "code", "text", "fixedasset", "fixed", "asset", "g", "l", "account",
    };

    /// <summary>Abbreviations AL field names use, and the word a caption here would use.</summary>
    private static readonly Dictionary<string, string> Synonyms = new(StringComparer.Ordinal)
    {
        ["contr"] = "contribution", ["contrib"] = "contribution", ["contributions"] = "contribution", ["int"] = "interest",
        ["amt"] = "amount", ["bal"] = "balance", ["desc"] = "description", ["nos"] = "no", ["number"] = "no", ["num"] = "no",
        ["ee"] = "employee", ["er"] = "employer", ["emp"] = "employee", ["reg"] = "registered", ["unreg"] = "unregistered",
        ["dob"] = "birth", ["doc"] = "document", ["qty"] = "quantity", ["depr"] = "depreciation", ["acc"] = "accumulated",
        ["nbv"] = "book", ["id"] = "id", ["pf"] = "no", ["staffno"] = "payroll", ["fullname"] = "name", ["names"] = "name",
        ["debit"] = "debit", ["credit"] = "credit", ["cost"] = "acquisition", ["additions"] = "additions", ["charge"] = "depreciation",
        ["salary"] = "salary", ["tittle"] = "title", ["appointment"] = "employment", ["appointmet"] = "employment",
    };

    [GeneratedRegex(@"^\s*\d+(\.\d+)?(cm|mm|in|pt|pc)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex LengthPattern();

    [GeneratedRegex(@"^\s*(#[0-9a-fA-F]{3,8}|[a-zA-Z]{3,24})\s*$")]
    private static partial Regex ColourPattern();

    [GeneratedRegex(@"^[a-zA-Z0-9 ,\-]{1,60}$")]
    private static partial Regex FontPattern();

    [GeneratedRegex(@"Fields!(\w+)\.Value", RegexOptions.IgnoreCase)]
    private static partial Regex FieldReference();

    [GeneratedRegex(@"(?<=[a-z0-9])(?=[A-Z])|[^A-Za-z0-9]+")]
    private static partial Regex WordBoundary();

    public static RdlcConversion Convert(string rdlcXml, IReadOnlyList<ReportColumnDefinition> reportColumns)
    {
        Check.NotNull(reportColumns, nameof(reportColumns));

        var report = Load(rdlcXml);
        var notes = new List<string>();

        var table = FindMainTable(report);
        var columns = table == null ? [] : MatchColumns(table, reportColumns, notes);

        if (table == null)
        {
            notes.Add("The layout has no table of detail rows, so the report's own columns are used with the layout's page setup and fonts.");
        }
        else if (columns.All(c => c.RdlcField == null || c.ReportColumnKey == null))
        {
            notes.Add("None of the layout's columns match a column of this report, so the report's own columns are used with the layout's page setup and fonts.");
            columns = [];
        }

        if (Descendants(report, "Image").Any())
        {
            notes.Add("Images (such as the company logo) are not carried over.");
        }

        if (Descendants(report, "Subreport").Any() || Descendants(report, "Chart").Any())
        {
            notes.Add("Sub-reports and charts are not carried over.");
        }

        if (Descendants(report, "Tablix").Count() > 1)
        {
            notes.Add("The layout has more than one table; only the one with the detail rows is carried over.");
        }

        return new RdlcConversion { Template = BuildTemplate(report, table, columns), Columns = columns, Notes = notes };
    }

    // ------------------------------------------------------------------ reading the RDLC

    private static XElement Load(string rdlcXml)
    {
        if (rdlcXml.IsNullOrWhiteSpace())
        {
            throw NotAnRdlc();
        }

        try
        {
            // DTDs are refused outright: a layout has no use for entities, and they are how an XML
            // file reads other files or exhausts memory.
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = MaxRdlcBytes * 2L };

            using var reader = XmlReader.Create(new StringReader(rdlcXml.TrimStart('﻿')), settings);
            var root = XDocument.Load(reader).Root;

            return root?.Name.LocalName == "Report" ? root : throw NotAnRdlc();
        }
        catch (XmlException)
        {
            throw NotAnRdlc();
        }
    }

    private static BusinessException NotAnRdlc() => new(ErpErrorCodes.Reports.RdlcNotValid);

    private static IEnumerable<XElement> Descendants(XElement element, string name) =>
        element.Descendants().Where(e => e.Name.LocalName == name);

    private static XElement Child(XElement element, string name) =>
        element?.Elements().FirstOrDefault(e => e.Name.LocalName == name);

    private static string ChildValue(XElement element, string name) => Child(element, name)?.Value;

    /// <summary>A table's rows as lists of cells; a cell merged into the one before it is absent.</summary>
    private sealed record Table(XElement Element, List<string> Widths, List<List<XElement>> Rows, int HeaderRow, int DetailRow, int TotalRow);

    /// <summary>The table whose detail row shows the most dataset fields: the body of a list report.</summary>
    private static Table FindMainTable(XElement report)
    {
        Table best = null;
        var bestFields = 0;

        foreach (var tablix in Descendants(report, "Tablix"))
        {
            var rows = Descendants(tablix, "TablixRow")
                .Select(row => Descendants(row, "TablixCell").ToList())
                .ToList();

            for (var i = 0; i < rows.Count; i++)
            {
                var fields = rows[i].Count(cell => IsDetailCell(ValueOf(cell)));
                if (fields <= bestFields)
                {
                    continue;
                }

                var widths = Descendants(tablix, "TablixColumn").Select(c => ChildValue(c, "Width")).ToList();
                var header = Enumerable.Range(0, i).LastOrDefault(r => rows[r].Count(cell => IsCaptionCell(ValueOf(cell))) >= Math.Max(1, fields / 2), -1);
                var total = Enumerable.Range(i + 1, rows.Count - i - 1).FirstOrDefault(r => rows[r].Any(cell => ValueOf(cell).Contains("Sum(", StringComparison.OrdinalIgnoreCase)), -1);

                best = new Table(tablix, widths, rows, header, i, total);
                bestFields = fields;
            }
        }

        return bestFields >= 2 ? best : null;
    }

    /// <summary>The expression or text a cell shows.</summary>
    private static string ValueOf(XElement cell) => Descendants(cell, "Value").FirstOrDefault()?.Value?.Trim() ?? string.Empty;

    /// <summary>A cell that shows one dataset field as it is: what a detail row is made of.</summary>
    private static bool IsDetailCell(string value)
    {
        return value.StartsWith('=')
            && FieldReference().IsMatch(value)
            && !value.Contains("Caption", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("Sum(", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("ToBase64String", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>A cell that shows a heading: plain text, or a caption field.</summary>
    private static bool IsCaptionCell(string value)
    {
        return value.Length > 0 && (!value.StartsWith('=') || value.Contains("Caption", StringComparison.OrdinalIgnoreCase));
    }

    // ------------------------------------------------------------------ matching columns

    private static List<RdlcColumnMatch> MatchColumns(Table table, IReadOnlyList<ReportColumnDefinition> reportColumns, List<string> notes)
    {
        var detail = table.Rows[table.DetailRow];
        var header = table.HeaderRow >= 0 ? table.Rows[table.HeaderRow] : [];

        var candidates = new List<(int Index, string Field, string Caption, bool CaptionIsText)>();
        for (var i = 0; i < detail.Count; i++)
        {
            var value = ValueOf(detail[i]);
            if (!IsDetailCell(value))
            {
                continue;
            }

            var field = FieldReference().Match(value).Groups[1].Value;
            var heading = i < header.Count ? ValueOf(header[i]) : string.Empty;
            var isText = heading.Length > 0 && !heading.StartsWith('=');

            var derived = Humanize(FieldReference().Match(heading).Groups[1].Value);
            candidates.Add((i, field, isText ? heading : derived.Length > 0 ? derived : Humanize(field), isText));
        }

        // Every pairing is scored, and the best taken first, so that a strong match is not lost to
        // a column that merely came earlier.
        var scores = (
            from candidate in candidates
            from column in reportColumns
            let score = Math.Max(Similarity(candidate.Caption, column), Similarity(candidate.Field, column))
            where score >= MatchThreshold
            orderby score descending
            select (candidate.Index, column.Key, score)
        ).ToList();

        var keyByIndex = new Dictionary<int, string>();
        var usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (index, key, _) in scores)
        {
            if (!keyByIndex.ContainsKey(index) && usedKeys.Add(key))
            {
                keyByIndex[index] = key;
            }
        }

        var matches = new List<RdlcColumnMatch>();
        foreach (var candidate in candidates)
        {
            var key = keyByIndex.GetValueOrDefault(candidate.Index);
            var column = key == null ? null : reportColumns.First(c => c.Key == key);

            if (column == null)
            {
                notes.Add($"The column \"{candidate.Caption.Trim()}\" ({candidate.Field}) has no counterpart in this report and was left out.");
            }

            matches.Add(
                new RdlcColumnMatch
                {
                    // The layout's own wording where it wrote one; otherwise the report's.
                    Caption = candidate.CaptionIsText || column == null ? candidate.Caption.Trim() : column.Header,
                    RdlcField = candidate.Field,
                    ReportColumnKey = key,
                }
            );
        }

        foreach (var column in reportColumns.Where(c => !usedKeys.Contains(c.Key)))
        {
            notes.Add($"The report's column \"{column.Header}\" is not in the layout and was added at the end.");
            matches.Add(new RdlcColumnMatch { Caption = column.Header, RdlcField = null, ReportColumnKey = column.Key });
        }

        return matches;
    }

    /// <summary>How alike a caption or field name and a report column are, from 0 to 1, by the words they share.</summary>
    private static double Similarity(string text, ReportColumnDefinition column)
    {
        var left = Words(text);
        var right = Words(column.Header).Union(Words(column.Key)).ToHashSet(StringComparer.Ordinal);

        if (left.Count == 0 || right.Count == 0)
        {
            return 0d;
        }

        // Dice's coefficient against the header's words; the key only adds words to match on, so
        // that a terse header ("No.") is not penalised for a descriptive key ("memberNo").
        var shared = left.Count(right.Contains);
        return shared == 0 ? 0d : Math.Min(1d, 2d * shared / (left.Count + Math.Max(1, Words(column.Header).Count)));
    }

    private static HashSet<string> Words(string text)
    {
        var words = new HashSet<string>(StringComparer.Ordinal);

        foreach (var raw in WordBoundary().Split(text ?? string.Empty))
        {
            var word = raw.ToLowerInvariant();
            if (word.Length == 0 || Noise.Contains(word))
            {
                continue;
            }

            words.Add(Synonyms.GetValueOrDefault(word, word.Length > 3 && word.EndsWith('s') ? word[..^1] : word));
        }

        return words;
    }

    /// <summary>"Vendor__Date_of_Birth_Caption" as "Date of Birth".</summary>
    private static string Humanize(string fieldName)
    {
        var words = WordBoundary()
            .Split(fieldName ?? string.Empty)
            .Where(w => w.Length > 0 && !w.Equals("Caption", StringComparison.OrdinalIgnoreCase) && !w.Equals("Lbl", StringComparison.OrdinalIgnoreCase));

        return string.Join(' ', words);
    }

    // ------------------------------------------------------------------ writing the layout

    private static string BuildTemplate(XElement report, Table table, List<RdlcColumnMatch> columns)
    {
        var page = Descendants(report, "Page").FirstOrDefault() ?? report;
        var detailStyle = table == null ? null : StyleOf(table.Rows[table.DetailRow].FirstOrDefault());
        var headerStyle = table is not { HeaderRow: >= 0 } ? null : StyleOf(table.Rows[table.HeaderRow].FirstOrDefault(c => ValueOf(c).Length > 0));
        var totalStyle = table is not { TotalRow: >= 0 } ? null : StyleOf(table.Rows[table.TotalRow].FirstOrDefault(c => ValueOf(c).Length > 0));

        var font = Font(detailStyle?.GetValueOrDefault("FontFamily")) ?? "Segoe UI, Helvetica, Arial, sans-serif";
        var fontSize = Length(detailStyle?.GetValueOrDefault("FontSize")) ?? "9pt";

        var html = new StringBuilder();
        html.Append("<!DOCTYPE html>\n<html>\n<head>\n<meta charset=\"utf-8\">\n<title>{{Title}}</title>\n<style>\n");

        var width = Length(ChildValue(page, "PageWidth"));
        var height = Length(ChildValue(page, "PageHeight"));
        var margins = new[] { "TopMargin", "RightMargin", "BottomMargin", "LeftMargin" }.Select(m => Length(ChildValue(page, m)) ?? "1.5cm");
        html.Append(CultureInfo.InvariantCulture, $"  @page {{ {(width != null && height != null ? $"size: {width} {height}; " : string.Empty)}margin: {string.Join(' ', margins)}; }}\n");
        html.Append(CultureInfo.InvariantCulture, $"  body {{ font-family: {font}; font-size: {fontSize}; color: #212529; margin: 1.5rem; }}\n");
        html.Append("  header { margin-bottom: 1rem; }\n  .company { font-size: 1.3em; font-weight: 600; }\n");
        html.Append(CultureInfo.InvariantCulture, $"  h1 {{ {Css(HeadingStyle(report), "font-size: 1.2em; font-weight: 600;")} margin: .3rem 0 0; }}\n");
        html.Append("  .period { color: #6c757d; margin-top: .2rem; }\n  table { width: 100%; border-collapse: collapse; }\n");
        html.Append(CultureInfo.InvariantCulture, $"  th {{ {Css(headerStyle, "font-weight: 600;")} text-align: left; border-bottom: 1px solid #adb5bd; padding: 2pt 4pt; }}\n");
        html.Append("  td { padding: 2pt 4pt; border-bottom: 1px solid #f1f3f5; }\n  th.number, td.number { text-align: right; font-variant-numeric: tabular-nums; }\n  td.date { white-space: nowrap; }\n");
        html.Append(CultureInfo.InvariantCulture, $"  tr.total td {{ {Css(totalStyle, "font-weight: 600;")} border-top: 1px solid #adb5bd; }}\n");
        html.Append("  tr.reversed td { font-style: italic; color: #6c757d; }\n");
        for (var level = 1; level <= 5; level++)
        {
            html.Append(CultureInfo.InvariantCulture, $"  tr.indent-{level} td:first-child {{ padding-left: {level * 1.5}rem; }}\n");
        }

        html.Append("  footer { margin-top: 1.2rem; color: #6c757d; font-size: .85em; }\n  @media print { body { margin: 0; } }\n</style>\n</head>\n<body>\n");

        // The heading is the layout's own wording where it has one, and the report's title otherwise.
        var heading = HeadingText(report);
        html.Append("<header>\n  <div class=\"company\">{{CompanyName}}</div>\n");
        html.Append(CultureInfo.InvariantCulture, $"  <h1>{(heading == null ? "{{Title}}" : Encode(heading))}</h1>\n");
        html.Append("  <div class=\"period\">{{FromDate}} &ndash; {{ToDate}}</div>\n</header>\n<table>\n");

        if (columns.Count == 0)
        {
            html.Append("  <thead>\n    <tr>{{#Columns}}<th class=\"{{Kind}}\">{{Header}}</th>{{/Columns}}</tr>\n  </thead>\n  <tbody>\n");
            html.Append("    {{#Rows}}<tr class=\"{{RowClass}} indent-{{Indent}}\">{{#Cells}}<td class=\"{{Kind}}\">{{Value}}</td>{{/Cells}}</tr>\n    {{/Rows}}\n");
        }
        else
        {
            var shown = columns.Where(c => c.ReportColumnKey != null).ToList();
            var detail = table.Rows[table.DetailRow];

            html.Append("  <colgroup>");
            foreach (var column in shown)
            {
                var index = column.RdlcField == null ? -1 : detail.FindIndex(cell => FieldReference().Match(ValueOf(cell)).Groups[1].Value == column.RdlcField);
                var columnWidth = index >= 0 && index < table.Widths.Count ? Length(table.Widths[index]) : null;
                html.Append(columnWidth == null ? "<col>" : $"<col style=\"width: {columnWidth}\">");
            }

            html.Append("</colgroup>\n  <thead>\n    <tr>");
            foreach (var column in shown)
            {
                html.Append(CultureInfo.InvariantCulture, $"<th>{Encode(column.Caption)}</th>");
            }

            html.Append("</tr>\n  </thead>\n  <tbody>\n    {{#Rows}}<tr class=\"{{RowClass}} indent-{{Indent}}\">");
            foreach (var column in shown)
            {
                var index = column.RdlcField == null ? -1 : detail.FindIndex(cell => FieldReference().Match(ValueOf(cell)).Groups[1].Value == column.RdlcField);
                var align = index < 0 ? null : StyleOf(detail[index]).GetValueOrDefault("TextAlign");
                var cellClass = string.Equals(align, "Right", StringComparison.OrdinalIgnoreCase) ? " class=\"number\"" : string.Empty;
                html.Append(CultureInfo.InvariantCulture, $"<td{cellClass}>{{{{Cell:{column.ReportColumnKey}}}}}</td>");
            }

            html.Append("</tr>\n    {{/Rows}}\n");
        }

        html.Append("  </tbody>\n</table>\n<footer>Printed {{PrintedOn}}</footer>\n</body>\n</html>\n");
        return html.ToString();
    }

    /// <summary>The style properties of the first text box in a cell, by RDLC name.</summary>
    private static Dictionary<string, string> StyleOf(XElement cell)
    {
        var style = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (cell == null)
        {
            return style;
        }

        foreach (var property in Descendants(cell, "Style").SelectMany(s => s.Elements()))
        {
            if (!property.HasElements && !property.Value.TrimStart().StartsWith('='))
            {
                style.TryAdd(property.Name.LocalName, property.Value.Trim());
            }
        }

        return style;
    }

    /// <summary>The text box of the page header that reads as the report's heading: literal text, in the largest type.</summary>
    private static XElement HeadingBox(XElement report)
    {
        var header = Descendants(report, "PageHeader").FirstOrDefault();

        return header == null
            ? null
            : Descendants(header, "Textbox")
                .Where(box => ValueOf(box).Length > 2 && !ValueOf(box).StartsWith('='))
                .OrderByDescending(box => Points(StyleOf(box).GetValueOrDefault("FontSize")))
                .FirstOrDefault();
    }

    private static string HeadingText(XElement report)
    {
        var box = HeadingBox(report);
        return box == null ? null : ValueOf(box);
    }

    private static Dictionary<string, string> HeadingStyle(XElement report) => StyleOf(HeadingBox(report));

    private static double Points(string size)
    {
        return size != null && size.EndsWith("pt", StringComparison.OrdinalIgnoreCase) && double.TryParse(size[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var points)
            ? points
            : 0d;
    }

    /// <summary>The CSS of an RDLC style, keeping only values that have the shape they should.</summary>
    private static string Css(Dictionary<string, string> style, string fallback)
    {
        if (style == null || style.Count == 0)
        {
            return fallback;
        }

        var css = new StringBuilder();
        Add(css, "font-family", Font(style.GetValueOrDefault("FontFamily")));
        Add(css, "font-size", Length(style.GetValueOrDefault("FontSize")));
        Add(css, "color", Colour(style.GetValueOrDefault("Color")));
        Add(css, "background-color", Colour(style.GetValueOrDefault("BackgroundColor")));

        var weight = style.GetValueOrDefault("FontWeight");
        Add(css, "font-weight", weight == null ? null : weight.Equals("Bold", StringComparison.OrdinalIgnoreCase) ? "bold" : weight.Equals("Normal", StringComparison.OrdinalIgnoreCase) ? "normal" : null);

        return css.Length == 0 ? fallback : css.ToString();
    }

    private static void Add(StringBuilder css, string property, string value)
    {
        if (value != null)
        {
            css.Append(CultureInfo.InvariantCulture, $"{property}: {value}; ");
        }
    }

    private static string Length(string value) => value != null && LengthPattern().IsMatch(value) ? value.Trim().ToLowerInvariant() : null;

    private static string Colour(string value)
    {
        return value != null && ColourPattern().IsMatch(value) && !value.Trim().Equals("No Color", StringComparison.OrdinalIgnoreCase)
            ? value.Trim()
            : null;
    }

    private static string Font(string value) => value != null && FontPattern().IsMatch(value) ? value.Trim() + ", sans-serif" : null;

    /// <summary>Text as it may appear in a layout: escaped, and unable to open a placeholder.</summary>
    private static string Encode(string text) => WebUtility.HtmlEncode(text ?? string.Empty).Replace("{{", "{ {").Replace("}}", "} }");
}
