using System;
using ABPmicroservice.Erp.Finance;

namespace ABPmicroservice.Erp.Documents;

/// <summary>
/// The VAT of one document line under a VAT Posting Setup, as the line
/// calculation works it out (prices exclude VAT):
/// <list type="bullet">
/// <item>Normal VAT: the rate on the line amount, added to what the party pays.</item>
/// <item>Reverse charge: the same VAT, but self-assessed by the buyer, so the party pays the line amount only.</item>
/// <item>Full VAT: the whole line is VAT, with no base.</item>
/// </list>
/// </summary>
public readonly record struct LineVat(decimal Base, decimal Amount, decimal AmountIncludingVat)
{
    public static LineVat Calculate(decimal lineAmount, VatCalculationType calculationType, decimal vatPercent)
    {
        return calculationType switch
        {
            VatCalculationType.FullVat => new LineVat(0m, lineAmount, lineAmount),
            VatCalculationType.ReverseChargeVat => new LineVat(lineAmount, Round(lineAmount * vatPercent / 100m), lineAmount),
            _ => Normal(lineAmount, vatPercent),
        };
    }

    private static LineVat Normal(decimal lineAmount, decimal vatPercent)
    {
        var amount = Round(lineAmount * vatPercent / 100m);
        return new LineVat(lineAmount, amount, lineAmount + amount);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
