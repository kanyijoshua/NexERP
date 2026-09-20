using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// Writes rows as CSV following RFC 4180: a field containing a comma, a quote or a line break is
/// wrapped in quotes and its own quotes are doubled.
/// <para>
/// This matters more than it looks: a customer named "Smith, Ltd." would otherwise split into two
/// columns and quietly shift every field after it.
/// </para>
/// </summary>
public static class DelimitedWriter
{
    public static string Write(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object>> rows, char separator = ',')
    {
        var builder = new StringBuilder();

        AppendRow(builder, headers, separator);

        foreach (var row in rows)
        {
            var values = new object[headers.Count];
            for (var i = 0; i < headers.Count; i++)
            {
                values[i] = i < row.Count ? row[i] : null;
            }

            AppendRow(builder, Array.ConvertAll(values, Format), separator);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Values are written in the invariant culture, so a decimal never turns into a comma that
    /// breaks the column layout, and a date is always readable by the other system.
    /// </summary>
    public static string Format(object value)
    {
        return value switch
        {
            null => string.Empty,
            bool flag => flag ? "true" : "false",
            DateTime date => date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateTimeOffset offset => offset.ToString("yyyy-MM-dd HH:mm:sszzz", CultureInfo.InvariantCulture),
            decimal number => number.ToString(CultureInfo.InvariantCulture),
            double number => number.ToString(CultureInfo.InvariantCulture),
            float number => number.ToString(CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    public static string Escape(string value, char separator)
    {
        value ??= string.Empty;

        var needsQuotes =
            value.IndexOf(separator) >= 0
            || value.Contains('"')
            || value.Contains('\n')
            || value.Contains('\r');

        return needsQuotes ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }

    private static void AppendRow(StringBuilder builder, IReadOnlyList<string> values, char separator)
    {
        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(separator);
            }

            builder.Append(Escape(values[i], separator));
        }

        builder.Append("\r\n");
    }
}
