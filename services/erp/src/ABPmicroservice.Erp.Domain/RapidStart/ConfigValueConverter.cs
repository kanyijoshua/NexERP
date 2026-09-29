using System;
using System.Globalization;
using ABPmicroservice.Erp.Exporting;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// Turns the text of a cell into a field's value and back.
/// <para>
/// Everything a package or an import carries is text — a CSV cell, an Excel cell, a JSON string —
/// so this is the one place that decides what "yes", "1500.5" or "45292" means for a given field.
/// Text is always read in the invariant culture, which is also how every export writes it, so a
/// file exported here is read back exactly.
/// </para>
/// </summary>
public static class ConfigValueConverter
{
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
        "yyyy-MM-ddTHH:mm:ssK",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
    ];

    /// <summary>
    /// Converts <paramref name="text"/> to the field's type. Blank means no value: null where the
    /// field allows it, the type's default (0, false) where it does not.
    /// </summary>
    public static bool TryConvert(ErpEntityField field, string text, out object value)
    {
        value = null;
        var type = field.ClrType;

        if (text.IsNullOrWhiteSpace())
        {
            value = field.IsNullable ? null : Activator.CreateInstance(type);
            return true;
        }

        var trimmed = text.Trim();

        if (type == typeof(string))
        {
            value = trimmed;
            return true;
        }

        if (type.IsEnum)
        {
            // By name, as exports write it, or by number when the number is a defined value.
            if (Enum.TryParse(type, trimmed, ignoreCase: true, out var parsed) && Enum.IsDefined(type, parsed))
            {
                value = parsed;
                return true;
            }

            return false;
        }

        if (type == typeof(bool))
        {
            switch (trimmed.ToLowerInvariant())
            {
                case "true" or "yes" or "y" or "1" or "x":
                    value = true;
                    return true;
                case "false" or "no" or "n" or "0":
                    value = false;
                    return true;
                default:
                    return false;
            }
        }

        if (type == typeof(Guid))
        {
            var ok = Guid.TryParse(trimmed, out var guid);
            value = guid;
            return ok;
        }

        if (type == typeof(DateTime))
        {
            if (TryParseDate(trimmed, out var date))
            {
                value = date;
                return true;
            }

            return false;
        }

        if (type == typeof(DateTimeOffset))
        {
            var ok = DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var offset);
            value = offset;
            return ok;
        }

        if (type == typeof(TimeSpan))
        {
            var ok = TimeSpan.TryParse(trimmed, CultureInfo.InvariantCulture, out var span);
            value = span;
            return ok;
        }

        if (type == typeof(decimal))
        {
            var ok = decimal.TryParse(trimmed, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var number);
            value = number;
            return ok;
        }

        if (type == typeof(double) || type == typeof(float))
        {
            var ok = double.TryParse(trimmed, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var number);
            value = type == typeof(float) ? (float)number : number;
            return ok;
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
        {
            // Excel stores every number as a double, so "10" may arrive as "10.0".
            if (!decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var number) || number != decimal.Truncate(number))
            {
                return false;
            }

            try
            {
                value = Convert.ChangeType(number, type, CultureInfo.InvariantCulture);
                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// An ISO date as exports write it, or an Excel serial number, which is what a date cell holds
    /// once a sheet has been opened and saved in Excel.
    /// </summary>
    public static bool TryParseDate(string text, out DateTime date)
    {
        if (DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date))
        {
            return true;
        }

        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial) && serial is > 0 and < 2_958_466)
        {
            date = SpreadsheetWriter.ExcelEpoch.AddDays(serial);
            return true;
        }

        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date);
    }

    /// <summary>A value as it is written into a package: invariant, and an enum by its name.</summary>
    public static string ToText(object value)
    {
        return value switch
        {
            null => null,
            DateTime date when date.TimeOfDay == TimeSpan.Zero => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Enum e => e.ToString(),
            _ => DelimitedWriter.Format(value),
        };
    }
}
