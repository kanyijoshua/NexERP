using ABPmicroservice.Erp.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// General Journal Batch. Mirrors Business Central table 232 "Gen. Journal Batch".
/// </summary>
public class GenJournalBatch : CompanyAggregateRoot
{
    public string JournalTemplateName { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    protected GenJournalBatch() { }

    public GenJournalBatch(Guid id, string journalTemplateName, string name, string description = null)
        : base(id)
    {
        JournalTemplateName = Check.NotNullOrWhiteSpace(journalTemplateName, nameof(journalTemplateName), ErpDomainConsts.MaxNameLength);
        SetName(name);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
    }
}

/// <summary>
/// General Journal Line. Mirrors Business Central table 81 "Gen. Journal Line".
/// </summary>
public class GenJournalLine : CompanyEntity
{
    public Guid GenJournalBatchId { get; private set; }
    public int LineNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public GLEntryDocumentType DocumentType { get; private set; }
    public string DocumentNo { get; private set; }
    public string AccountType { get; private set; } // "G/L Account", "Customer", "Vendor"
    public string AccountNo { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public string BalAccountType { get; private set; } // "G/L Account", "Customer", "Vendor"
    public string BalAccountNo { get; private set; }
    public Guid DimensionSetId { get; private set; }

    protected GenJournalLine() { }

    public GenJournalLine(
        Guid id,
        Guid genJournalBatchId,
        int lineNo,
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string accountType,
        string accountNo,
        string description,
        decimal amount,
        string balAccountType = null,
        string balAccountNo = null,
        Guid dimensionSetId = default
    )
        : base(id)
    {
        GenJournalBatchId = genJournalBatchId;
        LineNo = lineNo;
        PostingDate = postingDate;
        DocumentType = documentType;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        AccountType = Check.NotNullOrWhiteSpace(accountType, nameof(accountType));
        AccountNo = Check.NotNullOrWhiteSpace(accountNo, nameof(accountNo), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;
        BalAccountType = balAccountType;
        BalAccountNo = Check.Length(balAccountNo, nameof(balAccountNo), ErpDomainConsts.MaxNoLength);
        DimensionSetId = dimensionSetId;
    }
}
