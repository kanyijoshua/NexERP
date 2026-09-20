using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Posted G/L Entry. Mirrors Business Central table 17 "G/L Entry".
/// Entries are immutable once created (posted); only the reversal bookkeeping may still change.
/// </summary>
public class GLEntry : LedgerEntryBase
{
    public Guid GLAccountId { get; private set; }

    public string GLAccountNo { get; private set; }

    public DateTime PostingDate { get; private set; }

    /// <summary>Date on the source document. Defaults to the posting date.</summary>
    public DateTime DocumentDate { get; private set; }

    public GLEntryDocumentType DocumentType { get; private set; }

    public string DocumentNo { get; private set; }

    public string Description { get; private set; }

    /// <summary>Positive = debit, negative = credit (BC convention).</summary>
    public decimal Amount { get; private set; }

    public string SourceNo { get; private set; }

    public string GenBusPostingGroup { get; private set; }

    /// <summary>
    /// Groups every entry written by one posting run, across ledgers. Mirrors BC "Transaction No.",
    /// which is the unit a reversal works on.
    /// </summary>
    public long TransactionNo { get; internal set; }

    /// <summary>Number of the <see cref="GLRegister"/> this entry was posted in.</summary>
    public long RegisterNo { get; internal set; }

    public string SourceCode { get; internal set; }

    public string ReasonCode { get; internal set; }

    public Guid DimensionSetId { get; private set; }

    /// <summary>True once a reversal has cancelled this entry. Mirrors BC "Reversed".</summary>
    public bool Reversed { get; internal set; }

    /// <summary>Entry number of the correction that reversed this one.</summary>
    public long ReversedByEntryNo { get; internal set; }

    /// <summary>Set on a correction entry: the entry number it reverses.</summary>
    public long ReversedEntryNo { get; internal set; }

    protected GLEntry() { }

    public GLEntry(
        Guid id,
        Guid glAccountId,
        string glAccountNo,
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string description,
        decimal amount,
        string sourceNo = null,
        string genBusPostingGroup = null,
        Guid dimensionSetId = default,
        DateTime? documentDate = null
    )
        : base(id)
    {
        GLAccountId = glAccountId;
        GLAccountNo = Check.NotNullOrWhiteSpace(
            glAccountNo,
            nameof(glAccountNo),
            ErpDomainConsts.MaxNoLength
        );
        PostingDate = postingDate;
        DocumentDate = documentDate ?? postingDate;
        DocumentType = documentType;
        DocumentNo = Check.Length(
            documentNo,
            nameof(documentNo),
            ErpDomainConsts.MaxDocumentNoLength
        );
        Description = Check.Length(
            description,
            nameof(description),
            ErpDomainConsts.MaxDescriptionLength
        );
        Amount = amount;
        SourceNo = Check.Length(sourceNo, nameof(sourceNo), ErpDomainConsts.MaxNoLength);
        GenBusPostingGroup = Check.Length(
            genBusPostingGroup,
            nameof(genBusPostingGroup),
            ErpDomainConsts.MaxGeneralBusPostingGroupLength
        );
        DimensionSetId = dimensionSetId;
    }

    /// <summary>Debit side of the entry, as a trial balance shows it.</summary>
    public decimal DebitAmount => Amount > 0m ? Amount : 0m;

    /// <summary>Credit side of the entry, reported positive.</summary>
    public decimal CreditAmount => Amount < 0m ? -Amount : 0m;
}
