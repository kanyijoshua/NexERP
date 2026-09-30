using System;
using Volo.Abp;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// A customer, vendor or employee ledger entry that payments can be applied to. Mirrors the
/// fields Business Central's "Cust. Entry-Apply Posted Entries" works on.
/// </summary>
public interface IApplicableLedgerEntry
{
    long EntryNo { get; }

    string DocumentNo { get; }

    /// <summary>Null for LCY.</summary>
    string CurrencyCode { get; }

    /// <summary>What is still open, in the entry's currency.</summary>
    decimal RemainingAmount { get; }

    /// <summary>What is still open, in LCY at the rate it is currently carried at.</summary>
    decimal RemainingAmountLcy { get; }

    bool Open { get; }

    /// <summary>Takes an applied amount off the entry, closing it when nothing is left.</summary>
    void ReduceRemaining(decimal amount, decimal amountLcy, long closedByEntryNo);
}

/// <summary>
/// Applies a new entry (usually a payment) to an open one (usually an invoice). Mirrors the core
/// of Business Central codeunit 12's application: each side gives up the applied amount, and
/// whatever LCY is left over because the two were booked at different rates is the realized gain
/// or loss.
/// </summary>
public static class EntryApplication
{
    /// <summary>
    /// Applies <paramref name="newEntry"/> to <paramref name="openEntry"/> and returns the realized
    /// difference in LCY: positive is a loss for the company, negative a gain. The control account
    /// still holds that amount once both sides are settled, so the caller moves it to the currency's
    /// gain or loss account.
    /// </summary>
    public static decimal Apply(IApplicableLedgerEntry newEntry, IApplicableLedgerEntry openEntry)
    {
        Check.NotNull(newEntry, nameof(newEntry));
        Check.NotNull(openEntry, nameof(openEntry));

        // A payment settles an invoice in the same currency, and only one that points the other way.
        if (
            !openEntry.Open
            || !string.Equals(newEntry.CurrencyCode, openEntry.CurrencyCode, StringComparison.OrdinalIgnoreCase)
            || Math.Sign(newEntry.RemainingAmount) == Math.Sign(openEntry.RemainingAmount)
            || newEntry.RemainingAmount == 0m
        )
        {
            throw new BusinessException(ErpErrorCodes.Journals.AppliesToEntryMismatch)
                .WithData("documentNo", openEntry.DocumentNo);
        }

        // The applied amount, as it comes off the open entry.
        var applied = Math.Sign(openEntry.RemainingAmount)
            * Math.Min(Math.Abs(openEntry.RemainingAmount), Math.Abs(newEntry.RemainingAmount));

        var openLcy = Portion(openEntry, applied);
        var newLcy = Portion(newEntry, -applied);

        openEntry.ReduceRemaining(applied, openLcy, newEntry.EntryNo);
        newEntry.ReduceRemaining(-applied, newLcy, openEntry.EntryNo);

        return openLcy + newLcy;
    }

    // The LCY share of an applied amount; all of it when the entry is settled in full, so no cent is stranded.
    private static decimal Portion(IApplicableLedgerEntry entry, decimal amount)
    {
        return amount == entry.RemainingAmount
            ? entry.RemainingAmountLcy
            : CurrencyExchangeRateManager.RoundLcy(entry.RemainingAmountLcy * amount / entry.RemainingAmount);
    }
}
