using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// General Journal Batch. Mirrors Business Central table 232 "Gen. Journal Batch".
/// A batch is one worksheet of journal lines that are checked and posted together.
/// </summary>
public class GenJournalBatch : CompanyAggregateRoot
{
    /// <summary>Name of the <see cref="GenJournalTemplate"/> this batch belongs to.</summary>
    public string JournalTemplateName { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    /// <summary>Stamped on posted entries. Mirrors BC "Reason Code".</summary>
    public string ReasonCode { get; private set; }

    /// <summary>Number series the document numbers of new lines are taken from.</summary>
    public string NoSeriesCode { get; private set; }

    /// <summary>
    /// Balancing account applied to lines that leave theirs blank. Mirrors BC "Bal. Account No.",
    /// which is how the cash-receipt and payment journals point at the bank account once.
    /// </summary>
    public GenJournalAccountType? BalAccountType { get; private set; }

    public string BalAccountNo { get; private set; }

    protected GenJournalBatch() { }

    public GenJournalBatch(
        Guid id,
        string journalTemplateName,
        string name,
        string description = null,
        string reasonCode = null,
        string noSeriesCode = null,
        GenJournalAccountType? balAccountType = null,
        string balAccountNo = null
    )
        : base(id)
    {
        JournalTemplateName = GenJournalTemplate.NormalizeName(journalTemplateName);
        SetName(name);
        Update(description, reasonCode, noSeriesCode, balAccountType, balAccountNo);
    }

    public void SetName(string name)
    {
        Name = GenJournalTemplate.NormalizeName(name);
    }

    public void Update(
        string description,
        string reasonCode,
        string noSeriesCode,
        GenJournalAccountType? balAccountType,
        string balAccountNo
    )
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        ReasonCode = Check.Length(reasonCode, nameof(reasonCode), ErpDomainConsts.MaxReasonCodeLength)?.ToUpperInvariant();
        NoSeriesCode = Check.Length(noSeriesCode, nameof(noSeriesCode), ErpDomainConsts.MaxNoSeriesCodeLength)?.ToUpperInvariant();

        balAccountNo = Check.Length(balAccountNo, nameof(balAccountNo), ErpDomainConsts.MaxNoLength);

        // An account type without an account number would silently do nothing at posting time.
        BalAccountNo = balAccountNo.IsNullOrWhiteSpace() ? null : balAccountNo.Trim();
        BalAccountType = BalAccountNo == null ? null : balAccountType ?? GenJournalAccountType.GLAccount;
    }
}

/// <summary>
/// General Journal Line. Mirrors Business Central table 81 "Gen. Journal Line".
/// Lines live only until they are posted: posting writes the ledgers and clears the line,
/// except on a recurring journal, where the line stays and its date moves on.
/// </summary>
public class GenJournalLine : CompanyEntity
{
    public Guid GenJournalBatchId { get; private set; }

    public int LineNo { get; private set; }

    public DateTime PostingDate { get; private set; }

    /// <summary>Date on the counterparty's document. Defaults to the posting date.</summary>
    public DateTime DocumentDate { get; private set; }

    public GLEntryDocumentType DocumentType { get; private set; }

    public string DocumentNo { get; private set; }

    public string ExternalDocumentNo { get; private set; }

    public GenJournalAccountType AccountType { get; private set; }

    public string AccountNo { get; private set; }

    public string Description { get; private set; }

    /// <summary>Positive debits the account, negative credits it, as in Business Central.</summary>
    public decimal Amount { get; private set; }

    public GenJournalAccountType? BalAccountType { get; private set; }

    public string BalAccountNo { get; private set; }

    public Guid DimensionSetId { get; private set; }

    /// <summary>Set on lines of a recurring journal. <see cref="RecurringMethod.None"/> elsewhere.</summary>
    public RecurringMethod RecurringMethod { get; private set; }

    /// <summary>Date formula the posting date moves on by after posting, e.g. "1M" or "1M+CM".</summary>
    public string RecurringFrequency { get; private set; }

    /// <summary>After this date the recurring line is skipped. Mirrors BC "Expiration Date".</summary>
    public DateTime? ExpirationDate { get; private set; }

    /// <summary>Open customer or vendor entry this line settles. Mirrors BC "Applies-to Doc. No.".</summary>
    public string AppliesToDocNo { get; private set; }

    public string Comment { get; private set; }

    protected GenJournalLine() { }

    public GenJournalLine(
        Guid id,
        Guid genJournalBatchId,
        int lineNo,
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string documentNo,
        GenJournalAccountType accountType,
        string accountNo,
        string description,
        decimal amount,
        GenJournalAccountType? balAccountType = null,
        string balAccountNo = null,
        Guid dimensionSetId = default
    )
        : base(id)
    {
        GenJournalBatchId = genJournalBatchId;
        LineNo = lineNo;
        DimensionSetId = dimensionSetId;

        Update(
            postingDate,
            documentType,
            documentNo,
            accountType,
            accountNo,
            description,
            amount,
            balAccountType,
            balAccountNo
        );
    }

    public void Update(
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string documentNo,
        GenJournalAccountType accountType,
        string accountNo,
        string description,
        decimal amount,
        GenJournalAccountType? balAccountType = null,
        string balAccountNo = null,
        DateTime? documentDate = null,
        string externalDocumentNo = null,
        string appliesToDocNo = null,
        string comment = null
    )
    {
        PostingDate = postingDate;
        DocumentDate = documentDate ?? postingDate;
        DocumentType = documentType;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        ExternalDocumentNo = Check.Length(
            externalDocumentNo,
            nameof(externalDocumentNo),
            ErpDomainConsts.MaxExternalDocumentNoLength
        );
        AccountType = accountType;
        AccountNo = Check.NotNullOrWhiteSpace(accountNo, nameof(accountNo), ErpDomainConsts.MaxNoLength).Trim();
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;

        balAccountNo = Check.Length(balAccountNo, nameof(balAccountNo), ErpDomainConsts.MaxNoLength);
        BalAccountNo = balAccountNo.IsNullOrWhiteSpace() ? null : balAccountNo.Trim();
        BalAccountType = BalAccountNo == null ? null : balAccountType ?? GenJournalAccountType.GLAccount;

        AppliesToDocNo = Check.Length(appliesToDocNo, nameof(appliesToDocNo), ErpDomainConsts.MaxDocumentNoLength);
        Comment = Check.Length(comment, nameof(comment), ErpDomainConsts.MaxCommentLength);
    }

    /// <summary>Turns the line into a recurring one. Only allowed under a recurring template.</summary>
    public void SetRecurring(RecurringMethod method, string frequency, DateTime? expirationDate)
    {
        if (method == RecurringMethod.None)
        {
            RecurringMethod = RecurringMethod.None;
            RecurringFrequency = null;
            ExpirationDate = null;
            return;
        }

        if (frequency.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Journals.RecurringFrequencyRequired);
        }

        if (!DateFormula.TryParse(frequency, out var parsed))
        {
            throw new BusinessException(ErpErrorCodes.Journals.InvalidDateFormula).WithData("formula", frequency);
        }

        RecurringMethod = method;
        RecurringFrequency = parsed.Text;
        ExpirationDate = expirationDate;
    }

    public void SetDimensionSet(Guid dimensionSetId) => DimensionSetId = dimensionSetId;

    /// <summary>True while the recurring line is still due to be posted on that date.</summary>
    public bool IsExpiredOn(DateTime date) => ExpirationDate.HasValue && date > ExpirationDate.Value;

    public bool IsReversing =>
        RecurringMethod is RecurringMethod.ReversingFixed or RecurringMethod.ReversingVariable;

    /// <summary>
    /// Moves a recurring line to its next period. Variable methods also blank the amount, so the
    /// next posting needs it typed again. Mirrors what BC's post batch does to a recurring line.
    /// </summary>
    internal void AdvanceRecurring()
    {
        var frequency = DateFormula.Parse(RecurringFrequency);
        PostingDate = frequency.Apply(PostingDate);
        DocumentDate = PostingDate;

        if (RecurringMethod is RecurringMethod.Variable or RecurringMethod.ReversingVariable)
        {
            Amount = 0m;
        }
    }
}
