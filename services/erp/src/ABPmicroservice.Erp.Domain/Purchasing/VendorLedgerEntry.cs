using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Vendor Ledger Entry.
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

    /// <summary>Groups the entries of one posting run.</summary>
    public long TransactionNo { get; internal set; }

    public long RegisterNo { get; internal set; }

    /// <summary>True once a reversal has cancelled this entry.</summary>
    public bool Reversed { get; internal set; }

    public long ReversedByEntryNo { get; internal set; }

    public long ReversedEntryNo { get; internal set; }

    public string VendorName { get; internal set; }
    public string VendorPostingGroup { get; internal set; }
    public string GlobalDimension1Code { get; internal set; }
    public string GlobalDimension2Code { get; internal set; }
    public string PurchaserCode { get; internal set; }
    public string UserId { get; internal set; }
    public string SourceCode { get; internal set; }
    public string OnHold { get; internal set; }
    public string AppliesToDocType { get; internal set; }
    public string AppliesToDocNo { get; internal set; }
    public string AppliesToId { get; internal set; }
    public string JournalBatchName { get; internal set; }
    public string ExternalDocumentNo { get; internal set; }
    public string PaymentMethodCode { get; internal set; }

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
        decimal? amountLcy = null,
        string vendorName = null,
        string vendorPostingGroup = null,
        string globalDimension1Code = null,
        string globalDimension2Code = null,
        string purchaserCode = null,
        string userId = null,
        string sourceCode = null,
        string onHold = null,
        string appliesToDocType = null,
        string appliesToDocNo = null,
        string appliesToId = null,
        string journalBatchName = null,
        string externalDocumentNo = null,
        string paymentMethodCode = null
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
        VendorName = Check.Length(vendorName, nameof(vendorName), ErpDomainConsts.MaxNameLength);
        VendorPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(vendorPostingGroup, nameof(vendorPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        GlobalDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension1Code, nameof(globalDimension1Code), ErpDomainConsts.MaxDimensionCodeLength));
        GlobalDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(globalDimension2Code, nameof(globalDimension2Code), ErpDomainConsts.MaxDimensionCodeLength));
        PurchaserCode = CodeTableEntity.NormalizeCode(Check.Length(purchaserCode, nameof(purchaserCode), ErpDomainConsts.MaxCodeLength));
        UserId = Check.Length(userId, nameof(userId), ErpDomainConsts.MaxUserNameLength);
        SourceCode = CodeTableEntity.NormalizeCode(Check.Length(sourceCode, nameof(sourceCode), ErpDomainConsts.MaxSourceCodeLength));
        OnHold = Check.Length(onHold, nameof(onHold), ErpDomainConsts.MaxOnHoldLength);
        AppliesToDocType = Check.Length(appliesToDocType, nameof(appliesToDocType), ErpDomainConsts.MaxCodeLength);
        AppliesToDocNo = Check.Length(appliesToDocNo, nameof(appliesToDocNo), ErpDomainConsts.MaxDocumentNoLength);
        AppliesToId = Check.Length(appliesToId, nameof(appliesToId), ErpDomainConsts.MaxNoLength);
        JournalBatchName = CodeTableEntity.NormalizeCode(Check.Length(journalBatchName, nameof(journalBatchName), ErpDomainConsts.MaxJournalTemplateNameLength));
        ExternalDocumentNo = Check.Length(externalDocumentNo, nameof(externalDocumentNo), ErpDomainConsts.MaxExternalDocumentNoLength);
        PaymentMethodCode = CodeTableEntity.NormalizeCode(Check.Length(paymentMethodCode, nameof(paymentMethodCode), ErpDomainConsts.MaxPaymentMethodCodeLength));
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
