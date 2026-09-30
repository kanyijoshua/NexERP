using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// VAT Entry. Mirrors Business Central table 254: one row per VAT amount a posting produced, the
/// basis of the VAT return. <see cref="Base"/> and <see cref="Amount"/> carry the G/L sign: a sale
/// is negative (output VAT owed), a purchase positive (input VAT reclaimable).
/// </summary>
public class VatEntry : LedgerEntryBase
{
    public DateTime PostingDate { get; private set; }
    public DateTime DocumentDate { get; private set; }
    public GLEntryDocumentType DocumentType { get; private set; }
    public string DocumentNo { get; private set; }
    public VatEntryType Type { get; private set; }
    public decimal Base { get; private set; }
    public decimal Amount { get; private set; }
    public VatCalculationType VatCalculationType { get; private set; }

    /// <summary>The customer or vendor the entry concerns.</summary>
    public string BillToPayToNo { get; private set; }

    public string VatBusPostingGroup { get; private set; }
    public string VatProdPostingGroup { get; private set; }
    public string VatIdentifier { get; private set; }
    public decimal VatPercent { get; private set; }

    public long TransactionNo { get; internal set; }
    public long RegisterNo { get; internal set; }

    /// <summary>Set by a VAT settlement; the entry then no longer counts as open.</summary>
    public bool Closed { get; internal set; }

    /// <summary>The settlement entry that closed this one.</summary>
    public long ClosedByEntryNo { get; internal set; }

    public bool Reversed { get; internal set; }
    public long ReversedByEntryNo { get; internal set; }
    public long ReversedEntryNo { get; internal set; }

    protected VatEntry() { }

    public VatEntry(
        Guid id,
        DateTime postingDate,
        DateTime documentDate,
        GLEntryDocumentType documentType,
        string documentNo,
        VatEntryType type,
        decimal @base,
        decimal amount,
        VatPostingSetup setup,
        string billToPayToNo
    )
        : base(id)
    {
        Check.NotNull(setup, nameof(setup));

        PostingDate = postingDate;
        DocumentDate = documentDate;
        DocumentType = documentType;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        Type = type;
        Base = @base;
        Amount = amount;
        VatCalculationType = setup.VatCalculationType;
        VatBusPostingGroup = setup.VatBusPostingGroup;
        VatProdPostingGroup = setup.VatProdPostingGroup;
        VatIdentifier = setup.VatIdentifier;
        VatPercent = setup.VatPercent;
        BillToPayToNo = Check.Length(billToPayToNo, nameof(billToPayToNo), ErpDomainConsts.MaxNoLength);
    }

    /// <summary>
    /// A settlement entry: the opposite of what it settles for one VAT posting setup and type, so
    /// the settled entries and it net to nothing. It is born closed.
    /// </summary>
    internal VatEntry(
        Guid id,
        DateTime postingDate,
        string documentNo,
        VatEntry sample,
        decimal @base,
        decimal amount
    )
        : base(id)
    {
        PostingDate = postingDate;
        DocumentDate = postingDate;
        DocumentType = GLEntryDocumentType.None;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        Type = VatEntryType.Settlement;
        Base = @base;
        Amount = amount;
        VatCalculationType = sample.VatCalculationType;
        VatBusPostingGroup = sample.VatBusPostingGroup;
        VatProdPostingGroup = sample.VatProdPostingGroup;
        VatIdentifier = sample.VatIdentifier;
        VatPercent = sample.VatPercent;
        Closed = true;
    }

    /// <summary>The mirror of this entry a reversal writes.</summary>
    internal VatEntry(Guid id, VatEntry original)
        : base(id)
    {
        PostingDate = original.PostingDate;
        DocumentDate = original.DocumentDate;
        DocumentType = original.DocumentType;
        DocumentNo = original.DocumentNo;
        Type = original.Type;
        Base = -original.Base;
        Amount = -original.Amount;
        VatCalculationType = original.VatCalculationType;
        VatBusPostingGroup = original.VatBusPostingGroup;
        VatProdPostingGroup = original.VatProdPostingGroup;
        VatIdentifier = original.VatIdentifier;
        VatPercent = original.VatPercent;
        BillToPayToNo = original.BillToPayToNo;
        ReversedEntryNo = original.EntryNo;
        Reversed = true;
    }
}
