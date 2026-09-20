using System;
using System.Collections.Generic;
using System.Linq;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// The account filter people type into an account schedule's Totaling field, and into the
/// account filter of a report request page.
/// <para>
/// The syntax is Business Central's: alternatives separated by <c>|</c>, each either a single
/// account number or a range written <c>from..to</c>. "1000..1999|2100" is every account from
/// 1000 to 1999 plus 2100. Account numbers are compared as text, as BC compares codes.
/// </para>
/// </summary>
public sealed class AccountTotaling
{
    private readonly Part[] _parts;

    public string Text { get; }

    private AccountTotaling(string text, Part[] parts)
    {
        Text = text;
        _parts = parts;
    }

    /// <summary>Matches nothing, which is what an empty filter means for a total.</summary>
    public static AccountTotaling Empty { get; } = new(string.Empty, Array.Empty<Part>());

    /// <summary>Matches every account. Used when a report is run without an account filter.</summary>
    public static AccountTotaling All { get; } = new("*", new[] { new Part(null, null) });

    public bool IsEmpty => _parts.Length == 0;

    public static AccountTotaling Parse(string text)
    {
        if (text.IsNullOrWhiteSpace())
        {
            return Empty;
        }

        var parts = new List<Part>();

        // Commas are accepted alongside BC's pipe: people type both.
        foreach (var alternative in text.Split(['|', ','], StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = alternative.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var rangeAt = trimmed.IndexOf("..", StringComparison.Ordinal);
            if (rangeAt < 0)
            {
                parts.Add(new Part(trimmed, trimmed));
                continue;
            }

            var from = trimmed[..rangeAt].Trim();
            var to = trimmed[(rangeAt + 2)..].Trim();

            // "1000.." and "..1999" are open-ended, as in BC.
            parts.Add(new Part(from.Length == 0 ? null : from, to.Length == 0 ? null : to));
        }

        return parts.Count == 0 ? Empty : new AccountTotaling(text.Trim(), parts.ToArray());
    }

    public bool Matches(string accountNo)
    {
        if (accountNo == null)
        {
            return false;
        }

        return _parts.Any(p => p.Matches(accountNo));
    }

    public override string ToString() => Text;

    private readonly record struct Part(string From, string To)
    {
        public bool Matches(string accountNo)
        {
            if (From != null && string.CompareOrdinal(accountNo, From) < 0)
            {
                return false;
            }

            // An account number longer than the upper bound but starting with it, such as "19990"
            // against "1999", is still inside the range, which is how BC reads a code range.
            if (To != null && string.CompareOrdinal(accountNo, To) > 0 && !accountNo.StartsWith(To, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }
}
