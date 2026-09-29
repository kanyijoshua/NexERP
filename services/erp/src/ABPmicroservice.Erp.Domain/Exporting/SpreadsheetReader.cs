using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Volo.Abp;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>One worksheet as read: its tab name and its rows of cell text.</summary>
public sealed class SpreadsheetSheetData
{
    public SpreadsheetSheetData(string name, List<List<string>> rows)
    {
        Name = name;
        Rows = rows;
    }

    public string Name { get; }

    /// <summary>
    /// Rows from the top of the sheet, each as wide as its right-most filled cell. A gap in the
    /// sheet is an empty string, so a column keeps its position.
    /// </summary>
    public List<List<string>> Rows { get; }
}

/// <summary>
/// Reads the cell values of an .xlsx file.
/// <para>
/// The counterpart of <see cref="SpreadsheetWriter"/>, and hand-written for the same reason: a
/// configuration package or an import needs plain values out of plain sheets, not a spreadsheet
/// engine. Every value comes back as invariant-culture text — numbers as "1500.5", booleans as
/// "true"/"false" — and the importer converts it to the field's type, exactly as it does for CSV.
/// Dates arrive as Excel serial numbers, which the importer recognises for a date field.
/// </para>
/// <para>
/// The file is uploaded by a user, so it is read defensively: DTDs are refused (no entity
/// expansion), and no part is read past <see cref="ErpDomainConsts.MaxUnzippedPartBytes"/>
/// whatever size the archive claims for it.
/// </para>
/// </summary>
public static class SpreadsheetReader
{
    private const string MainNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string PackageRelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";

    private static readonly XNamespace Main = MainNamespace;

    /// <summary>The first bytes of every zip file, and so of every xlsx.</summary>
    public static bool LooksLikeSpreadsheet(byte[] content)
    {
        return content is { Length: > 4 } && content[0] == 0x50 && content[1] == 0x4B && content[2] == 0x03 && content[3] == 0x04;
    }

    public static List<SpreadsheetSheetData> Read(byte[] content)
    {
        try
        {
            using var archive = new ZipArchive(new MemoryStream(content), ZipArchiveMode.Read);

            var sharedStrings = ReadSharedStrings(archive);
            var targets = ReadSheetTargets(archive);

            return ReadSheetList(archive)
                .Select(sheet => new SpreadsheetSheetData(
                    sheet.Name,
                    targets.TryGetValue(sheet.RelationshipId, out var path) ? ReadRows(archive, path, sharedStrings) : []
                ))
                .ToList();
        }
        catch (Exception exception) when (exception is InvalidDataException or XmlException or FormatException)
        {
            throw NotValid();
        }
    }

    private static List<(string Name, string RelationshipId)> ReadSheetList(ZipArchive archive)
    {
        var sheets = new List<(string, string)>();

        using var reader = Open(archive, "xl/workbook.xml") ?? throw NotValid();
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "sheet" && reader.NamespaceURI == MainNamespace)
            {
                sheets.Add((reader.GetAttribute("name") ?? string.Empty, reader.GetAttribute("id", RelationshipNamespace)));
            }
        }

        return sheets;
    }

    /// <summary>Relationship id to part path, e.g. rId1 to "xl/worksheets/sheet1.xml".</summary>
    private static Dictionary<string, string> ReadSheetTargets(ZipArchive archive)
    {
        var targets = new Dictionary<string, string>(StringComparer.Ordinal);

        using var reader = Open(archive, "xl/_rels/workbook.xml.rels");
        if (reader == null)
        {
            return targets;
        }

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "Relationship" || reader.NamespaceURI != PackageRelationshipNamespace)
            {
                continue;
            }

            var id = reader.GetAttribute("Id");
            var target = reader.GetAttribute("Target");
            if (id == null || target == null)
            {
                continue;
            }

            // Targets are relative to xl/, or absolute from the package root when they start with "/".
            targets[id] = target.StartsWith('/') ? target.TrimStart('/') : "xl/" + target;
        }

        return targets;
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var strings = new List<string>();

        using var reader = Open(archive, "xl/sharedStrings.xml");
        if (reader == null)
        {
            return strings;
        }

        reader.MoveToContent();
        while (!reader.EOF)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "si" && reader.NamespaceURI == MainNamespace)
            {
                // ReadFrom leaves the reader on the node after the element, so no Read() here.
                strings.Add(TextOf((XElement)XNode.ReadFrom(reader)));
            }
            else
            {
                reader.Read();
            }
        }

        return strings;
    }

    /// <summary>
    /// Text of an &lt;si&gt; or &lt;is&gt; element: its &lt;t&gt; runs joined, leaving out the phonetic
    /// hints (&lt;rPh&gt;) Excel stores alongside Japanese text.
    /// </summary>
    private static string TextOf(XElement element)
    {
        if (element == null)
        {
            return string.Empty;
        }

        return string.Concat(
            element
                .Descendants(Main + "t")
                .Where(t => t.Parent?.Name != Main + "rPh")
                .Select(t => t.Value)
        );
    }

    private static List<List<string>> ReadRows(ZipArchive archive, string path, List<string> sharedStrings)
    {
        var rows = new List<List<string>>();

        using var reader = Open(archive, path);
        if (reader == null)
        {
            return rows;
        }

        reader.MoveToContent();
        while (!reader.EOF)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "row" || reader.NamespaceURI != MainNamespace)
            {
                reader.Read();
                continue;
            }

            var element = (XElement)XNode.ReadFrom(reader);

            // An empty row may be left out of the XML altogether; keep the numbering honest.
            var number = ParseRowNumber((string)element.Attribute("r")) ?? rows.Count + 1;
            while (rows.Count < number - 1)
            {
                rows.Add([]);
            }

            var row = new List<string>();
            foreach (var cell in element.Elements(Main + "c"))
            {
                var reference = (string)cell.Attribute("r");
                var column = reference == null ? row.Count : ColumnIndex(reference);

                while (row.Count <= column)
                {
                    row.Add(string.Empty);
                }

                row[column] = CellValue(cell, (string)cell.Attribute("t"), sharedStrings);
            }

            rows.Add(row);
        }

        // Trailing empty rows say nothing, and would otherwise read as blank records.
        while (rows.Count > 0 && rows[^1].All(string.IsNullOrWhiteSpace))
        {
            rows.RemoveAt(rows.Count - 1);
        }

        return rows;
    }

    private static string CellValue(XElement cell, string type, List<string> sharedStrings)
    {
        var raw = cell.Element(Main + "v")?.Value;

        switch (type)
        {
            case "s":
                return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)
                    && index >= 0
                    && index < sharedStrings.Count
                    ? sharedStrings[index]
                    : string.Empty;

            case "inlineStr":
                return TextOf(cell.Element(Main + "is"));

            case "b":
                return raw == "1" ? "true" : "false";

            default:
                // Numbers are stored in invariant form already; formula strings ("str") and errors
                // ("e") are taken as the text they show.
                return raw ?? string.Empty;
        }
    }

    private static int? ParseRowNumber(string value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) && number > 0
            ? number
            : null;
    }

    /// <summary>"A1" is column 0, "AA7" is column 26: the inverse of <see cref="SpreadsheetWriter.Reference"/>.</summary>
    public static int ColumnIndex(string reference)
    {
        var index = 0;

        foreach (var c in reference)
        {
            if (c is < 'A' or > 'Z')
            {
                break;
            }

            index = index * 26 + (c - 'A' + 1);
        }

        return Math.Max(0, index - 1);
    }

    private static XmlReader Open(ZipArchive archive, string path)
    {
        var entry = archive.Entries.FirstOrDefault(e => string.Equals(e.FullName, path, StringComparison.OrdinalIgnoreCase));
        if (entry == null)
        {
            return null;
        }

        var stream = new LimitedReadStream(entry.Open(), ErpDomainConsts.MaxUnzippedPartBytes);

        return XmlReader.Create(
            stream,
            new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                IgnoreWhitespace = false,
                CloseInput = true,
            }
        );
    }

    private static BusinessException NotValid() => new(ErpErrorCodes.RapidStart.FileNotValid);

    /// <summary>Fails the read once more than the allowed number of bytes has come out of the zip.</summary>
    private sealed class LimitedReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly long _limit;
        private long _read;

        public LimitedReadStream(Stream inner, long limit)
        {
            _inner = inner;
            _limit = limit;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _inner.Read(buffer, offset, count);
            _read += read;

            if (_read > _limit)
            {
                throw new BusinessException(ErpErrorCodes.RapidStart.FileTooLarge)
                    .WithData("maxSize", _limit / (1024 * 1024) + " MB");
            }

            return read;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => _read;
            set => throw new NotSupportedException();
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
