using System;

namespace ABPmicroservice.Erp.Finance;

/// <summary>The VAT a journal amount carries: the base the account gets and the VAT beside it.</summary>
public readonly record struct JournalVatAmounts(decimal Base, decimal Vat);

/// <summary>
/// VAT on a general journal line. Calculates the line's VAT fields:
/// a journal amount includes VAT (the amount paid or received), unlike a document line.
/// </summary>
public static class JournalVat
{
    /// <summary>
    /// Splits a VAT-inclusive amount. Normal VAT takes the VAT out of the amount; reverse charge
    /// adds self-assessed VAT on top of it (the supplier charged none); Full VAT is all VAT.
    /// A sale under reverse charge carries no VAT, as the customer accounts for it.
    /// </summary>
    public static JournalVatAmounts Calculate(decimal amount, VatPostingSetup setup, GeneralPostingType postingType)
    {
        if (setup == null || postingType == GeneralPostingType.None)
        {
            return new JournalVatAmounts(amount, 0m);
        }

        return setup.VatCalculationType switch
        {
            VatCalculationType.FullVat => new JournalVatAmounts(0m, amount),
            VatCalculationType.ReverseChargeVat => new JournalVatAmounts(
                amount,
                postingType == GeneralPostingType.Purchase ? Round(amount * setup.VatPercent / 100m) : 0m
            ),
            _ => Normal(amount, setup.VatPercent),
        };
    }

    private static JournalVatAmounts Normal(decimal amount, decimal percent)
    {
        var vat = Round(amount * percent / (100m + percent));
        return new JournalVatAmounts(amount - vat, vat);
    }

    private static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
