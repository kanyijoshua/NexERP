using System;
using System.Collections.Generic;
using System.Linq;
using ABPmicroservice.Erp.Exporting;
using Volo.Abp;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// Opens an uploaded file as sheets of text rows, whichever of the accepted formats it is in, and
/// matches its column headers to a table's fields.
/// </summary>
public static class ImportFileReader
{
    /// <summary>
    /// An .xlsx gives one sheet per tab; a CSV (or any text file) gives a single sheet named after
    /// the file. The format is told by content, not by the name, so a renamed file still opens.
    /// </summary>
    public static List<SpreadsheetSheetData> Read(string fileName, byte[] content, char? separator = null)
    {
        EnsureSize(content);

        if (SpreadsheetReader.LooksLikeSpreadsheet(content))
        {
            return SpreadsheetReader.Read(content);
        }

        var name = System.IO.Path.GetFileNameWithoutExtension(fileName ?? string.Empty);
        return [new SpreadsheetSheetData(name.IsNullOrWhiteSpace() ? "Sheet1" : name, DelimitedReader.Read(content, separator))];
    }

    public static void EnsureSize(byte[] content)
    {
        if (content == null || content.Length == 0)
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.FileHasNoRows);
        }

        if (content.Length > ErpDomainConsts.MaxImportFileBytes)
        {
            throw new BusinessException(ErpErrorCodes.RapidStart.FileTooLarge)
                .WithData("maxSize", ErpDomainConsts.MaxImportFileBytes / (1024 * 1024) + " MB");
        }
    }

    /// <summary>
    /// The field a column header names, or null. A header may be the field's name ("PostCode"),
    /// its display name ("Post Code"), or either with different spacing and case:
    /// both the technical and the human name of a field are accepted. A reference field also answers
    /// to its name without "Id", so "Item Category" finds ItemCategoryId.
    /// </summary>
    public static string MatchField(ConfigTableProfile profile, string header)
    {
        var wanted = Normalize(header);
        if (wanted.Length == 0)
        {
            return null;
        }

        foreach (var field in profile.ImportableFields)
        {
            if (Normalize(field.Name) == wanted || Normalize(field.DisplayName) == wanted)
            {
                return field.Name;
            }
        }

        foreach (var relation in profile.Relations.Where(r => r.IsIdReference))
        {
            var withoutId = relation.FieldName.EndsWith("Id", StringComparison.Ordinal) ? relation.FieldName[..^2] : relation.FieldName;
            if (Normalize(withoutId) == wanted)
            {
                return relation.FieldName;
            }
        }

        return null;
    }

    /// <summary>The sheet that holds a table: by its name, or by its display name ("Customer Posting Group").</summary>
    public static bool SheetIsFor(SpreadsheetSheetData sheet, ConfigTableProfile profile)
    {
        var name = Normalize(sheet.Name);
        return name == Normalize(profile.Name) || name == Normalize(SpreadsheetWriter.SheetName(profile.Name));
    }

    /// <summary>Rows below the header row as field name to value, skipping columns that match no field.</summary>
    public static List<Dictionary<string, string>> ToRecords(IReadOnlyList<List<string>> rows, IReadOnlyList<string> columnFields)
    {
        var records = new List<Dictionary<string, string>>();

        foreach (var row in rows)
        {
            if (row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var column = 0; column < columnFields.Count; column++)
            {
                var field = columnFields[column];
                if (field != null)
                {
                    record[field] = column < row.Count ? row[column] : string.Empty;
                }
            }

            records.Add(record);
        }

        return records;
    }

    private static string Normalize(string text)
    {
        return new string((text ?? string.Empty).Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    }
}
