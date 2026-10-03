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

    /// <summary>G/L Account Name snapshot. Mirrors BC field 76 "G/L Account Name".</summary>
    public string GLAccountName { get; internal set; }

    /// <summary>Gen. Posting Type. Mirrors BC field 48 "Gen. Posting Type".</summary>
    public GeneralPostingType GenPostingType { get; internal set; }

    /// <summary>Gen. Product Posting Group. Mirrors BC field 50 "Gen. Prod. Posting Group".</summary>
    public string GenProdPostingGroup { get; internal set; }

    /// <summary>Balancing Account Type. Mirrors BC field 51 "Bal. Account Type".</summary>
    public GenJournalAccountType BalAccountType { get; internal set; }

    /// <summary>Balancing Account No. Mirrors BC field 10 "Bal. Account No.".</summary>
    public string BalAccountNo { get; internal set; }

    /// <summary>VAT Amount. Mirrors BC field 43 "VAT Amount".</summary>
    public decimal VATAmount { get; internal set; }

    /// <summary>VAT Bus. Posting Group. Mirrors BC field 64 "VAT Bus. Posting Group".</summary>
    public string VATBusPostingGroup { get; internal set; }

    /// <summary>VAT Prod. Posting Group. Mirrors BC field 65 "VAT Prod. Posting Group".</summary>
    public string VATProdPostingGroup { get; internal set; }

    /// <summary>External Document No. Mirrors BC field 56 "External Document No.".</summary>
    public string ExternalDocumentNo { get; internal set; }

    /// <summary>Source Type. Mirrors BC field 57 "Source Type".</summary>
    public GLEntrySourceType SourceType { get; internal set; }

    /// <summary>User ID that posted the entry. Mirrors BC field 27 "User ID".</summary>
    public string UserId { get; internal set; }

    /// <summary>Journal Batch Name. Mirrors BC field 46 "Journal Batch Name".</summary>
    public string JournalBatchName { get; internal set; }

    /// <summary>Quantity. Mirrors BC field 42 "Quantity".</summary>
    public decimal Quantity { get; internal set; }

    /// <summary>Additional-Currency Amount. Mirrors BC field 68 "Additional-Currency Amount".</summary>
    public decimal AdditionalCurrencyAmount { get; internal set; }

    /// <summary>Job No. / Project No. Mirrors BC field 41 "Job No.".</summary>
    public string JobNo { get; internal set; }

    /// <summary>Business Unit Code. Mirrors BC field 45 "Business Unit Code".</summary>
    public string BusinessUnitCode { get; internal set; }

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
        DateTime? documentDate = null,
        string glAccountName = null,
        GeneralPostingType genPostingType = GeneralPostingType.None,
        string genProdPostingGroup = null,
        GenJournalAccountType balAccountType = GenJournalAccountType.GLAccount,
        string balAccountNo = null,
        decimal vatAmount = 0m,
        string vatBusPostingGroup = null,
        string vatProdPostingGroup = null,
        string externalDocumentNo = null,
        GLEntrySourceType sourceType = GLEntrySourceType.None,
        string userId = null,
        string journalBatchName = null,
        decimal quantity = 0m,
        decimal additionalCurrencyAmount = 0m,
        string jobNo = null,
        string businessUnitCode = null
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

        GLAccountName = Check.Length(glAccountName, nameof(glAccountName), ErpDomainConsts.MaxNameLength);
        GenPostingType = genPostingType;
        GenProdPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(genProdPostingGroup, nameof(genProdPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        BalAccountType = balAccountType;
        BalAccountNo = CodeTableEntity.NormalizeCode(Check.Length(balAccountNo, nameof(balAccountNo), ErpDomainConsts.MaxNoLength));
        VATAmount = vatAmount;
        VATBusPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(vatBusPostingGroup, nameof(vatBusPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        VATProdPostingGroup = CodeTableEntity.NormalizeCode(Check.Length(vatProdPostingGroup, nameof(vatProdPostingGroup), ErpDomainConsts.MaxPostingGroupLength));
        ExternalDocumentNo = Check.Length(externalDocumentNo, nameof(externalDocumentNo), ErpDomainConsts.MaxExternalDocumentNoLength);
        SourceType = sourceType;
        UserId = Check.Length(userId, nameof(userId), ErpDomainConsts.MaxUserNameLength);
        JournalBatchName = Check.Length(journalBatchName, nameof(journalBatchName), ErpDomainConsts.MaxJournalTemplateNameLength);
        Quantity = quantity;
        AdditionalCurrencyAmount = additionalCurrencyAmount;
        JobNo = CodeTableEntity.NormalizeCode(Check.Length(jobNo, nameof(jobNo), ErpDomainConsts.MaxNoLength));
        BusinessUnitCode = CodeTableEntity.NormalizeCode(Check.Length(businessUnitCode, nameof(businessUnitCode), ErpDomainConsts.MaxCodeLength));
    }

    /// <summary>Debit side of the entry, as a trial balance shows it.</summary>
    public decimal DebitAmount => Amount > 0m ? Amount : 0m;

    /// <summary>Credit side of the entry, reported positive.</summary>
    public decimal CreditAmount => Amount < 0m ? -Amount : 0m;
}
