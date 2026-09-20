using System;
using System.Globalization;
using System.Text;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Business Central date formula, as typed into "Recurring Frequency" and payment terms.
/// <para>
/// A formula is a sequence of terms applied left to right to a date. A term is an optional sign,
/// an optional count and a unit: <c>D</c> day, <c>W</c> week, <c>M</c> month, <c>Q</c> quarter,
/// <c>Y</c> year. A <c>C</c> in front of the unit means "the end of the current unit" instead of
/// an offset, so <c>CM</c> is the last day of the month.
/// </para>
/// <para>
/// "1M" is a month later, "-7D" a week earlier, and "1M+CM" the last day of next month, which is
/// the usual frequency of a monthly accrual.
/// </para>
/// </summary>
public sealed class DateFormula
{
    private readonly Term[] _terms;

    public string Text { get; }

    private DateFormula(string text, Term[] terms)
    {
        Text = text;
        _terms = terms;
    }

    public static bool TryParse(string text, out DateFormula formula)
    {
        formula = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.Trim().ToUpperInvariant().Replace(" ", string.Empty);
        var terms = new System.Collections.Generic.List<Term>();
        var index = 0;

        while (index < normalized.Length)
        {
            var sign = 1;
            if (normalized[index] == '+' || normalized[index] == '-')
            {
                sign = normalized[index] == '-' ? -1 : 1;
                index++;
            }

            var digits = new StringBuilder();
            while (index < normalized.Length && char.IsDigit(normalized[index]))
            {
                digits.Append(normalized[index]);
                index++;
            }

            if (index >= normalized.Length)
            {
                return false;
            }

            var toEndOfPeriod = false;
            if (normalized[index] == 'C')
            {
                toEndOfPeriod = true;
                index++;
                if (index >= normalized.Length)
                {
                    return false;
                }
            }

            var unit = normalized[index];
            if (unit is not ('D' or 'W' or 'M' or 'Q' or 'Y'))
            {
                return false;
            }
            index++;

            var count = digits.Length == 0 ? (toEndOfPeriod ? 0 : 1) : int.Parse(digits.ToString(), CultureInfo.InvariantCulture);
            terms.Add(new Term(sign * count, unit, toEndOfPeriod));
        }

        if (terms.Count == 0)
        {
            return false;
        }

        formula = new DateFormula(normalized, terms.ToArray());
        return true;
    }

    public static DateFormula Parse(string text)
    {
        return TryParse(text, out var formula)
            ? formula
            : throw new ArgumentException($"'{text}' is not a valid date formula.", nameof(text));
    }

    public DateTime Apply(DateTime date)
    {
        var result = date;
        foreach (var term in _terms)
        {
            result = term.Apply(result);
        }

        return result;
    }

    public override string ToString() => Text;

    private readonly record struct Term(int Count, char Unit, bool ToEndOfPeriod)
    {
        public DateTime Apply(DateTime date)
        {
            var moved = Unit switch
            {
                'D' => date.AddDays(Count),
                'W' => date.AddDays(Count * 7),
                'M' => date.AddMonths(Count),
                'Q' => date.AddMonths(Count * 3),
                'Y' => date.AddYears(Count),
                _ => date,
            };

            return ToEndOfPeriod ? EndOfPeriod(moved) : moved;
        }

        private DateTime EndOfPeriod(DateTime date)
        {
            switch (Unit)
            {
                case 'D':
                    return date;
                case 'W':
                    // The BC week ends on Sunday.
                    var daysToSunday = ((int)DayOfWeek.Sunday - (int)date.DayOfWeek + 7) % 7;
                    return date.AddDays(daysToSunday);
                case 'M':
                    return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 0, 0, 0, date.Kind);
                case 'Q':
                    var lastMonthOfQuarter = ((date.Month - 1) / 3 + 1) * 3;
                    return new DateTime(
                        date.Year,
                        lastMonthOfQuarter,
                        DateTime.DaysInMonth(date.Year, lastMonthOfQuarter),
                        0,
                        0,
                        0,
                        date.Kind
                    );
                case 'Y':
                    return new DateTime(date.Year, 12, 31, 0, 0, 0, date.Kind);
                default:
                    return date;
            }
        }
    }
}
