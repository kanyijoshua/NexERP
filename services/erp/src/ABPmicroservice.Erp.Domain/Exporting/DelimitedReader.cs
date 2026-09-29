using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// Reads CSV following RFC 4180, the counterpart of <see cref="DelimitedWriter"/>: a quoted field
/// may hold the separator, doubled quotes and line breaks.
/// <para>
/// Odoo's import asks for the separator and guesses it when left blank; this does the same. A file
/// saved from Excel in a comma-decimal locale uses semicolons, and one exported from many tools
/// uses tabs, so the separator is taken from whichever of the three the header line uses most.
/// </para>
/// </summary>
public static class DelimitedReader
{
    private static readonly char[] Candidates = [',', ';', '\t'];

    public static List<List<string>> Read(byte[] content, char? separator = null)
    {
        return Read(Decode(content), separator);
    }

    public static List<List<string>> Read(string text, char? separator = null)
    {
        text ??= string.Empty;
        var delimiter = separator ?? Detect(text);

        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }

                continue;
            }

            if (c == '"' && field.Length == 0)
            {
                inQuotes = true;
            }
            else if (c == delimiter)
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if (c is '\r' or '\n')
            {
                row.Add(field.ToString());
                field.Clear();
                rows.Add(row);
                row = [];

                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }
            }
            else
            {
                field.Append(c);
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        // A blank line carries no record: most often it is the newline at the end of the file.
        rows.RemoveAll(r => r.All(string.IsNullOrWhiteSpace));
        return rows;
    }

    /// <summary>
    /// UTF-8, with or without the byte-order mark the writer adds for Excel. Bytes that are not
    /// valid UTF-8 are taken as Latin-1, which matches the Windows-1252 that Excel writes a "CSV"
    /// in on Western systems for every letter, so an accented name survives instead of turning
    /// into replacement characters.
    /// </summary>
    public static string Decode(byte[] content)
    {
        if (content == null || content.Length == 0)
        {
            return string.Empty;
        }

        var offset = content.Length >= 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF ? 3 : 0;

        try
        {
            return new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(content, offset, content.Length - offset);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Latin1.GetString(content, offset, content.Length - offset);
        }
    }

    /// <summary>The candidate separator that occurs most often outside quotes on the first line.</summary>
    public static char Detect(string text)
    {
        using var reader = new StringReader(text ?? string.Empty);
        var firstLine = reader.ReadLine() ?? string.Empty;

        var counts = Candidates.ToDictionary(c => c, _ => 0);
        var inQuotes = false;

        foreach (var c in firstLine)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (!inQuotes && counts.ContainsKey(c))
            {
                counts[c]++;
            }
        }

        var best = counts.OrderByDescending(p => p.Value).First();
        return best.Value == 0 ? ',' : best.Key;
    }
}
