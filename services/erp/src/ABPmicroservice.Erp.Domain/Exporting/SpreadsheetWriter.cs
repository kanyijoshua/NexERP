using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// Writes a single-sheet .xlsx file.
/// <para>
/// An xlsx is a zip of XML parts, so the format is written directly rather than pulling in a
/// spreadsheet library for one sheet of plain cells. Numbers and dates are written as numbers and
/// dates, not as text, so the file can be sorted and totalled the moment it opens — which is the
/// whole point of offering Excel next to CSV.
/// </para>
/// </summary>
public static class SpreadsheetWriter
{
    /// <summary>Excel counts days from 1899-12-30 because of a deliberate leap-year bug in 1900.</summary>
    private static readonly DateTime ExcelEpoch = new(1899, 12, 30);

    public static byte[] Write(string sheetName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object>> rows)
    {
        using var buffer = new MemoryStream();

        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml", ContentTypes);
            AddEntry(archive, "_rels/.rels", RootRelationships);
            AddEntry(archive, "xl/workbook.xml", Workbook(sheetName));
            AddEntry(archive, "xl/_rels/workbook.xml.rels", WorkbookRelationships);
            AddEntry(archive, "xl/styles.xml", Styles);
            AddEntry(archive, "xl/worksheets/sheet1.xml", Sheet(headers, rows));
        }

        return buffer.ToArray();
    }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string Sheet(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object>> rows)
    {
        var sheet = new StringBuilder();
        sheet.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""");
        sheet.Append(
            """<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>"""
        );

        var rowIndex = 1;

        sheet.Append($"<row r=\"{rowIndex}\">");
        for (var column = 0; column < headers.Count; column++)
        {
            sheet.Append(TextCell(Reference(column, rowIndex), headers[column], styleIndex: 2));
        }

        sheet.Append("</row>");

        foreach (var row in rows)
        {
            rowIndex++;
            sheet.Append($"<row r=\"{rowIndex}\">");

            for (var column = 0; column < headers.Count; column++)
            {
                var value = column < row.Count ? row[column] : null;
                sheet.Append(Cell(Reference(column, rowIndex), value));
            }

            sheet.Append("</row>");
        }

        sheet.Append("</sheetData></worksheet>");
        return sheet.ToString();
    }

    private static string Cell(string reference, object value)
    {
        switch (value)
        {
            case null:
                return $"<c r=\"{reference}\"/>";

            case bool flag:
                return $"<c r=\"{reference}\" t=\"b\"><v>{(flag ? 1 : 0)}</v></c>";

            case DateTime date:
                var serial = (date - ExcelEpoch).TotalDays;
                return $"<c r=\"{reference}\" s=\"1\"><v>{serial.ToString("0.#####", CultureInfo.InvariantCulture)}</v></c>";

            case DateTimeOffset offset:
                return Cell(reference, offset.UtcDateTime);

            case decimal or double or float or int or long or short or byte:
                var number = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return $"<c r=\"{reference}\"><v>{number.ToString(CultureInfo.InvariantCulture)}</v></c>";

            default:
                return TextCell(reference, Convert.ToString(value, CultureInfo.InvariantCulture), styleIndex: 0);
        }
    }

    /// <summary>
    /// Text goes in as an inline string, which avoids a shared-string table and keeps the writer
    /// to one pass over the rows.
    /// </summary>
    private static string TextCell(string reference, string text, int styleIndex)
    {
        var style = styleIndex == 0 ? string.Empty : $" s=\"{styleIndex}\"";
        return $"<c r=\"{reference}\"{style} t=\"inlineStr\"><is><t xml:space=\"preserve\">{Escape(text)}</t></is></c>";
    }

    private static string Escape(string text)
    {
        if (text.IsNullOrEmpty())
        {
            return string.Empty;
        }

        // Control characters are not legal in XML 1.0 and would make the file unopenable.
        var cleaned = new string(text.Where(c => c >= 0x20 || c is '\t' or '\n' or '\r').ToArray());
        return SecurityElement.Escape(cleaned) ?? string.Empty;
    }

    /// <summary>Turns a zero-based column index and a row number into "A1", "B1", … "AA1".</summary>
    public static string Reference(int columnIndex, int rowNumber)
    {
        var name = string.Empty;
        var remaining = columnIndex;

        do
        {
            name = (char)('A' + remaining % 26) + name;
            remaining = remaining / 26 - 1;
        } while (remaining >= 0);

        return name + rowNumber.ToString(CultureInfo.InvariantCulture);
    }

    private const string ContentTypes = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
          <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
          <Default Extension="xml" ContentType="application/xml"/>
          <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
          <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
          <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
        </Types>
        """;

    private const string RootRelationships = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
        </Relationships>
        """;

    private const string WorkbookRelationships = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
          <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
        </Relationships>
        """;

    /// <summary>Three formats are enough: plain, a date and a bold header.</summary>
    private const string Styles = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
          <fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><name val="Calibri"/></font></fonts>
          <fills count="1"><fill><patternFill patternType="none"/></fill></fills>
          <borders count="1"><border/></borders>
          <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
          <cellXfs count="3">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
            <xf numFmtId="14" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/>
            <xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/>
          </cellXfs>
        </styleSheet>
        """;

    private static string Workbook(string sheetName)
    {
        var safeName = Escape(sheetName.IsNullOrWhiteSpace() ? "Sheet1" : sheetName);

        // Excel refuses a sheet name over 31 characters or containing : \ / ? * [ ]
        safeName = new string(safeName.Where(c => !":\\/?*[]".Contains(c)).ToArray());
        if (safeName.Length > 31)
        {
            safeName = safeName[..31];
        }

        return $"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <sheets><sheet name="{safeName}" sheetId="1" r:id="rId1"/></sheets>
            </workbook>
            """;
    }
}
