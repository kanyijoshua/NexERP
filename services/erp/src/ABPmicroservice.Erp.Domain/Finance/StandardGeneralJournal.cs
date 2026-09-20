using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Standard General Journal. Mirrors Business Central table 750 "Standard General Journal".
/// <para>
/// A saved copy of a batch's lines that can be dropped into the journal again. It is how BC
/// handles a monthly set of entries that is not regular enough to be a recurring journal.
/// </para>
/// </summary>
public class StandardGeneralJournal : CompanyAggregateRoot
{
    public string JournalTemplateName { get; private set; }

    /// <summary>Business key within the template. Mirrors BC "Code".</summary>
    public string Code { get; private set; }

    public string Description { get; private set; }

    public Collection<StandardGeneralJournalLine> Lines { get; private set; }

    protected StandardGeneralJournal()
    {
        Lines = new Collection<StandardGeneralJournalLine>();
    }

    public StandardGeneralJournal(Guid id, string journalTemplateName, string code, string description = null)
        : base(id)
    {
        JournalTemplateName = GenJournalTemplate.NormalizeName(journalTemplateName);
        Code = GenJournalTemplate.NormalizeName(code);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Lines = new Collection<StandardGeneralJournalLine>();
    }

    public void SetDescription(string description)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    /// <summary>Replaces the saved lines with a snapshot of the journal lines given.</summary>
    public void SaveFrom(IEnumerable<GenJournalLine> journalLines, Func<Guid> newId)
    {
        Lines.Clear();

        var lineNo = 0;
        foreach (var line in journalLines)
        {
            Lines.Add(
                new StandardGeneralJournalLine(
                    newId(),
                    Id,
                    ++lineNo,
                    line.DocumentType,
                    line.AccountType,
                    line.AccountNo,
                    line.Description,
                    line.Amount,
                    line.BalAccountType,
                    line.BalAccountNo
                )
            );
        }
    }
}

/// <summary>
/// Standard General Journal Line. Mirrors Business Central table 751.
/// It carries no date or document number: those come from the journal it is copied into.
/// </summary>
public class StandardGeneralJournalLine : FullAuditedEntity<Guid>
{
    public Guid StandardGeneralJournalId { get; private set; }

    public int LineNo { get; private set; }

    public GLEntryDocumentType DocumentType { get; private set; }

    public GenJournalAccountType AccountType { get; private set; }

    public string AccountNo { get; private set; }

    public string Description { get; private set; }

    public decimal Amount { get; private set; }

    public GenJournalAccountType? BalAccountType { get; private set; }

    public string BalAccountNo { get; private set; }

    protected StandardGeneralJournalLine() { }

    public StandardGeneralJournalLine(
        Guid id,
        Guid standardGeneralJournalId,
        int lineNo,
        GLEntryDocumentType documentType,
        GenJournalAccountType accountType,
        string accountNo,
        string description,
        decimal amount,
        GenJournalAccountType? balAccountType,
        string balAccountNo
    )
        : base(id)
    {
        StandardGeneralJournalId = standardGeneralJournalId;
        LineNo = lineNo;
        DocumentType = documentType;
        AccountType = accountType;
        AccountNo = Check.NotNullOrWhiteSpace(accountNo, nameof(accountNo), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;
        BalAccountType = balAccountType;
        BalAccountNo = Check.Length(balAccountNo, nameof(balAccountNo), ErpDomainConsts.MaxNoLength);
    }
}
