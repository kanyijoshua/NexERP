using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>One column of an export: where to read it and what to call it.</summary>
public sealed class ExportColumn
{
    public ExportColumn(string key, string header)
    {
        Key = key;
        Header = header;
    }

    public string Key { get; }

    public string Header { get; }
}

/// <summary>A finished file, ready to be streamed to the caller.</summary>
public sealed class ExportFile
{
    public ExportFile(byte[] content, string fileName, string contentType)
    {
        Content = content;
        FileName = fileName;
        ContentType = contentType;
    }

    public byte[] Content { get; }

    public string FileName { get; }

    public string ContentType { get; }
}

/// <summary>
/// Turns rows into a file people can open.
/// <para>
/// Business Central exports its configuration packages to Excel; Odoo's list views export to CSV
/// or XLSX with a chosen set of fields. Both are the same job, so every export in this system —
/// a table, a report, a filtered list — comes through here and comes out consistent.
/// </para>
/// </summary>
public class DataExportEngine : ITransientDependency
{
    public ExportFile Write(
        string name,
        IReadOnlyList<ExportColumn> columns,
        IReadOnlyList<IReadOnlyDictionary<string, object>> rows,
        ExportFormat format
    )
    {
        var safeName = Sanitize(name);
        var headers = columns.Select(c => c.Header).ToList();
        var values = rows.Select(row => columns.Select(c => row.GetValueOrDefault(c.Key)).ToList()).ToList();

        return format switch
        {
            ExportFormat.Xlsx => new ExportFile(
                SpreadsheetWriter.Write(safeName, headers, values),
                $"{safeName}.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            ),
            ExportFormat.Json => new ExportFile(
                WriteJson(columns, rows),
                $"{safeName}.json",
                "application/json"
            ),
            _ => new ExportFile(
                WriteCsv(headers, values),
                $"{safeName}.csv",
                "text/csv"
            ),
        };
    }

    /// <summary>
    /// CSV carries a byte-order mark: without it Excel reads a UTF-8 file as the local code page
    /// and mangles every accented name.
    /// </summary>
    private static byte[] WriteCsv(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<object>> rows)
    {
        var text = DelimitedWriter.Write(headers, rows);
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetPreamble().Concat(Encoding.UTF8.GetBytes(text)).ToArray();
    }

    private static byte[] WriteJson(
        IReadOnlyList<ExportColumn> columns,
        IReadOnlyList<IReadOnlyDictionary<string, object>> rows
    )
    {
        // Keys stay as field names, not headers: JSON is read by another system, not by a person.
        var payload = rows.Select(row => columns.ToDictionary(c => c.Key, c => row.GetValueOrDefault(c.Key))).ToList();

        return JsonSerializer.SerializeToUtf8Bytes(
            payload,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }
        );
    }

    /// <summary>Keeps a file name to characters that survive a Content-Disposition header.</summary>
    public static string Sanitize(string name)
    {
        if (name.IsNullOrWhiteSpace())
        {
            return "export";
        }

        var cleaned = new string(name.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '-').ToArray())
            .Trim('-');

        return cleaned.Length == 0 ? "export" : cleaned[..Math.Min(cleaned.Length, 60)];
    }
}
