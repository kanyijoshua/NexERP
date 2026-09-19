using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Posted G/L Entry. Mirrors Business Central table 17 "G/L Entry".
/// Entries are immutable once created (posted).
/// </summary>
public class GLEntry : LedgerEntryBase
{
    /// <summary>Sequential entry number. Mirrors BC field "Entry No.".</summary>

    public Guid GLAccountId { get; private set; }

    public string GLAccountNo { get; private set; }

    public DateTime PostingDate { get; private set; }

    public GLEntryDocumentType DocumentType { get; private set; }

    public string DocumentNo { get; private set; }

    public string Description { get; private set; }

    /// <summary>Positive = debit, negative = credit (BC convention).</summary>
    public decimal Amount { get; private set; }

    public string SourceNo { get; private set; }

    public string GenBusPostingGroup { get; private set; }

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
        string genBusPostingGroup = null
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
    }
}
