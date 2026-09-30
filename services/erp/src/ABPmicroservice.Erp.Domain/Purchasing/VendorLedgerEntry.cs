using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Vendor Ledger Entry. Mirrors Business Central table 25 "Vendor Ledger Entry".
/// Immutable subledger entry tracking accounts payable balances per vendor.
/// </summary>
public class VendorLedgerEntry : LedgerEntryBase, IApplicableLedgerEntry
{
    public Guid VendorId { get; private set; }
    public string VendorNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string DocumentType { get; private set; } // Invoice, Payment, Credit Memo
    public string DocumentNo { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public decimal RemainingAmount { get; internal set; }

    /// <summary>The currency of <see cref="Amount"/> and <see cref="RemainingAmount"/>; null for LCY.</summary>
    public string CurrencyCode { get; private set; }

    /// <summary>The amount in LCY at the posting date's rate.</summary>
    public decimal AmountLcy { get; private set; }

    /// <summary>What is still open in LCY, at the rate of the last exchange rate adjustment.</summary>
    public decimal RemainingAmountLcy { get; internal set; }

    /// <summary>The entry that closed this one by application.</summary>
    public long ClosedByEntryNo { get; internal set; }
    public DateTime DueDate { get; private set; }
    public bool Open { get; internal set; }
    public Guid DimensionSetId { get; private set; }

    /// <summary>Groups the entries of one posting run. Mirrors BC "Transaction No.".</summary>
    public long TransactionNo { get; internal set; }

    public long RegisterNo { get; internal set; }

    /// <summary>True once a reversal has cancelled this entry.</summary>
    public bool Reversed { get; internal set; }

    public long ReversedByEntryNo { get; internal set; }

    public long ReversedEntryNo { get; internal set; }

    protected VendorLedgerEntry() { }

    public VendorLedgerEntry(
        Guid id,
        Guid vendorId,
        string vendorNo,
        DateTime postingDate,
        string documentType,
        string documentNo,
        string description,
        decimal amount,
        DateTime dueDate,
        Guid dimensionSetId = default,
        string currencyCode = null,
        decimal? amountLcy = null
    )
        : base(id)
    {
        VendorId = vendorId;
        VendorNo = Check.NotNullOrWhiteSpace(vendorNo, nameof(vendorNo), ErpDomainConsts.MaxNoLength);
        PostingDate = postingDate;
        DocumentType = documentType;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;
        RemainingAmount = amount;
        CurrencyCode = currencyCode;
        AmountLcy = amountLcy ?? amount;
        RemainingAmountLcy = AmountLcy;
        DueDate = dueDate;
        Open = true;
        DimensionSetId = dimensionSetId;
    }

    void IApplicableLedgerEntry.ReduceRemaining(decimal amount, decimal amountLcy, long closedByEntryNo)
    {
        RemainingAmount -= amount;
        RemainingAmountLcy -= amountLcy;
        if (RemainingAmount == 0m)
        {
            Open = false;
            RemainingAmountLcy = 0m;
            ClosedByEntryNo = closedByEntryNo;
        }
    }

    /// <summary>Carries the open amount at a new rate. Used by the exchange rate adjustment.</summary>
    internal void AdjustRemainingLcy(decimal remainingAmountLcy) => RemainingAmountLcy = remainingAmountLcy;
}
