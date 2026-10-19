using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Customer Ledger Entry.
/// Immutable subledger entry tracking accounts receivable balances per customer.
/// </summary>
public class CustomerLedgerEntry : LedgerEntryBase, IPartyLedgerEntry
{
    public Guid CustomerId { get; private set; }
    public string CustomerNo { get; private set; }
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

    /// <summary>Groups the entries of one posting run.</summary>
    public long TransactionNo { get; internal set; }

    public long RegisterNo { get; internal set; }

    /// <summary>True once a reversal has cancelled this entry.</summary>
    public bool Reversed { get; internal set; }

    public long ReversedByEntryNo { get; internal set; }

    public long ReversedEntryNo { get; internal set; }

    /// <summary>The highest reminder level issued for this entry; the next reminder takes the level after it.</summary>
    public int LastIssuedReminderLevel { get; internal set; }

    Guid IPartyLedgerEntry.PartyId => CustomerId;

    string IPartyLedgerEntry.PartyNo => CustomerNo;

    /// <summary>The last day a payment may take the discount on.</summary>
    public DateTime? PmtDiscountDate { get; private set; }

    /// <summary>The discount the entry offered when posted, in its currency.</summary>
    public decimal OriginalPmtDiscPossible { get; private set; }

    /// <summary>The discount still to be taken; zero once taken or once the entry is closed without it.</summary>
    public decimal RemainingPmtDiscPossible { get; internal set; }

    internal void SetPaymentDiscount(DateTime? pmtDiscountDate, decimal possible)
    {
        PmtDiscountDate = possible == 0m ? null : pmtDiscountDate?.Date;
        OriginalPmtDiscPossible = PmtDiscountDate == null ? 0m : possible;
        RemainingPmtDiscPossible = OriginalPmtDiscPossible;
    }

    void IPartyLedgerEntry.TakePmtDiscount() => RemainingPmtDiscPossible = 0m;

    void IPartyLedgerEntry.RestorePmtDiscount(decimal amount) => RemainingPmtDiscPossible = amount;

    void IApplicableLedgerEntry.RestoreRemaining(decimal amount, decimal amountLcy)
    {
        RemainingAmount += amount;
        RemainingAmountLcy += amountLcy;
        Open = RemainingAmount != 0m;
        if (Open)
        {
            ClosedByEntryNo = 0;
        }
    }

    protected CustomerLedgerEntry() { }

    public CustomerLedgerEntry(
        Guid id,
        Guid customerId,
        string customerNo,
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
        CustomerId = customerId;
        CustomerNo = Check.NotNullOrWhiteSpace(customerNo, nameof(customerNo), ErpDomainConsts.MaxNoLength);
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
