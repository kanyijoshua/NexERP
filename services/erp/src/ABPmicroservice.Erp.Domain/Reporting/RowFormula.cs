using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Volo.Abp;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>One term of a row formula: a row number and the sign it is added with.</summary>
public readonly record struct RowFormulaTerm(int Sign, string RowNo);

/// <summary>
/// The formula an account schedule row can hold instead of accounts, e.g. "R10+R20-R30".
/// Mirrors the Formula totaling type of Business Central's Acc. Schedule Line.
/// </summary>
public static class RowFormula
{
    /// <summary>
    /// Reads a formula as a signed list of row numbers. Anything unreadable is refused rather
    /// than silently treated as zero, because a mistyped row reference would quietly change a
    /// financial statement.
    /// </summary>
    public static List<RowFormulaTerm> Parse(string formula, string rowNo = null)
    {
        if (formula.IsNullOrWhiteSpace())
        {
            throw Invalid(rowNo);
        }

        var terms = new List<RowFormulaTerm>();
        var sign = 1;
        var token = new StringBuilder();

        // An operator with nothing after it means the user is not finished typing.
        var expectingTerm = false;

        void Flush()
        {
            if (token.Length == 0)
            {
                return;
            }

            terms.Add(new RowFormulaTerm(sign, token.ToString().Trim().ToUpperInvariant()));
            token.Clear();
        }

        foreach (var character in formula)
        {
            switch (character)
            {
                case '+':
                    Flush();
                    sign = 1;
                    expectingTerm = true;
                    break;
                case '-':
                    Flush();
                    sign = -1;
                    expectingTerm = true;
                    break;
                case ' ':
                    break;
                default:
                    token.Append(character);
                    expectingTerm = false;
                    break;
            }
        }

        Flush();

        if (expectingTerm || terms.Count == 0 || terms.Any(t => t.RowNo.Length == 0))
        {
            throw Invalid(rowNo);
        }

        return terms;
    }

    private static BusinessException Invalid(string rowNo)
    {
        return new BusinessException(ErpErrorCodes.Reports.InvalidRowFormula).WithData("rowNo", rowNo ?? string.Empty);
    }
}
