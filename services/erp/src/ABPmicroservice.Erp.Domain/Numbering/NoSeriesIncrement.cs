using System;
using System.Numerics;

namespace ABPmicroservice.Erp.Numbering;

/// <summary>
/// Number arithmetic for number series. Mirrors Business Central's INCSTR:
/// the LAST group of digits in the text is incremented and keeps its width,
/// so "SI-00099" + 1 is "SI-00100" and "2026-INV009A" + 1 is "2026-INV010A".
/// </summary>
public static class NoSeriesIncrement
{
    /// <summary>Returns null when the text holds no digits and so cannot be incremented.</summary>
    public static string Increment(string no, int incrementBy = 1)
    {
        if (string.IsNullOrEmpty(no) || incrementBy < 1 || !TryFindLastDigitGroup(no, out var start, out var length))
        {
            return null;
        }

        var digits = no.Substring(start, length);
        var next = (BigInteger.Parse(digits) + incrementBy).ToString();

        // Keep leading zeros; grow only when the number overflows its width (99 -> 100).
        return no.Substring(0, start) + next.PadLeft(length, '0') + no.Substring(start + length);
    }

    /// <summary>
    /// Orders two numbers of the same series: by the value of the last digit group when the
    /// surrounding text matches, otherwise by plain text as Business Central does.
    /// </summary>
    public static int Compare(string left, string right)
    {
        if (
            left != null
            && right != null
            && TryFindLastDigitGroup(left, out var ls, out var ll)
            && TryFindLastDigitGroup(right, out var rs, out var rl)
            && left.Substring(0, ls) == right.Substring(0, rs)
            && left.Substring(ls + ll) == right.Substring(rs + rl)
        )
        {
            return BigInteger.Parse(left.Substring(ls, ll)).CompareTo(BigInteger.Parse(right.Substring(rs, rl)));
        }

        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryFindLastDigitGroup(string text, out int start, out int length)
    {
        var end = text.Length - 1;
        while (end >= 0 && !char.IsAsciiDigit(text[end]))
        {
            end--;
        }

        if (end < 0)
        {
            start = length = 0;
            return false;
        }

        start = end;
        while (start > 0 && char.IsAsciiDigit(text[start - 1]))
        {
            start--;
        }

        length = end - start + 1;
        return true;
    }
}
