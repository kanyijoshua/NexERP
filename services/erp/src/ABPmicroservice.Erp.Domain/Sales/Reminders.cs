using System;
using System.Collections.Generic;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Sales;

/// <summary>Reminder Terms: how many reminders a customer gets and what each level costs.</summary>
public class ReminderTerms : CodeTableEntity
{
    protected override int MaxCodeLength => ErpDomainConsts.MaxReminderTermsCodeLength;

    /// <summary>Interest on a reminder is posted to the customer; otherwise it is only shown.</summary>
    public bool PostInterest { get; private set; }

    /// <summary>The fee of a reminder level is posted to the customer; otherwise it is only shown.</summary>
    public bool PostAdditionalFee { get; private set; }

    /// <summary>No reminder past this many for one entry; zero means no limit.</summary>
    public int MaxNoOfReminders { get; private set; }

    /// <summary>Overdue amounts below this are not reminded of.</summary>
    public decimal MinimumAmountLcy { get; private set; }

    protected ReminderTerms() { }

    public ReminderTerms(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetRules(bool postInterest, bool postAdditionalFee, int maxNoOfReminders, decimal minimumAmountLcy)
    {
        PostInterest = postInterest;
        PostAdditionalFee = postAdditionalFee;
        MaxNoOfReminders = Math.Max(0, maxNoOfReminders);
        MinimumAmountLcy = Math.Max(0m, minimumAmountLcy);
    }
}

/// <summary>Reminder Level: one step of reminder terms, with its grace period and fee.</summary>
public class ReminderLevel : CompanyEntity
{
    public string ReminderTermsCode { get; private set; }
    public int No { get; private set; }

    /// <summary>How long after the due date (or the previous reminder) before this level is sent, e.g. "5D".</summary>
    public string GracePeriod { get; private set; }

    /// <summary>When the reminder itself falls due, from its document date.</summary>
    public string DueDateCalculation { get; private set; }

    public decimal AdditionalFeeLcy { get; private set; }

    /// <summary>Charge interest on the overdue amounts, at the customer finance charge terms.</summary>
    public bool CalculateInterest { get; private set; }

    protected ReminderLevel() { }

    public ReminderLevel(Guid id, string reminderTermsCode, int no)
        : base(id)
    {
        ReminderTermsCode = Check.NotNullOrWhiteSpace(reminderTermsCode, nameof(reminderTermsCode), ErpDomainConsts.MaxReminderTermsCodeLength).Trim().ToUpperInvariant();
        SetNo(no);
    }

    public void SetNo(int no)
    {
        if (no < 1)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.ReminderLevelNotFound).WithData("level", no).WithData("terms", ReminderTermsCode);
        }

        No = no;
    }

    public void SetRules(string gracePeriod, string dueDateCalculation, decimal additionalFeeLcy, bool calculateInterest)
    {
        GracePeriod = Formula(gracePeriod);
        DueDateCalculation = Formula(dueDateCalculation);
        AdditionalFeeLcy = Math.Max(0m, additionalFeeLcy);
        CalculateInterest = calculateInterest;
    }

    internal static string Formula(string text)
    {
        if (text.IsNullOrWhiteSpace())
        {
            return null;
        }

        var trimmed = text.Trim().ToUpperInvariant();
        if (!DateFormula.TryParse(trimmed, out _))
        {
            throw new BusinessException(ErpErrorCodes.Journals.InvalidDateFormula).WithData("formula", trimmed);
        }

        return trimmed;
    }
}

/// <summary>Finance Charge Terms: the interest and fee charged on overdue amounts.</summary>
public class FinanceChargeTerms : CodeTableEntity
{
    protected override int MaxCodeLength => ErpDomainConsts.MaxReminderTermsCodeLength;

    /// <summary>Interest per <see cref="InterestPeriodDays"/>, in percent.</summary>
    public decimal InterestRate { get; private set; }

    /// <summary>The number of days the rate is for: 365 for a yearly rate, 30 for a monthly one.</summary>
    public int InterestPeriodDays { get; private set; } = 365;

    public decimal MinimumAmountLcy { get; private set; }

    public decimal AdditionalFeeLcy { get; private set; }

    /// <summary>Days after the due date before interest starts, e.g. "5D".</summary>
    public string GracePeriod { get; private set; }

    public string DueDateCalculation { get; private set; }

    public bool PostInterest { get; private set; } = true;

    public bool PostAdditionalFee { get; private set; } = true;

    protected FinanceChargeTerms() { }

    public FinanceChargeTerms(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetRules(
        decimal interestRate,
        int interestPeriodDays,
        decimal minimumAmountLcy,
        decimal additionalFeeLcy,
        string gracePeriod,
        string dueDateCalculation,
        bool postInterest,
        bool postAdditionalFee
    )
    {
        if (interestRate < 0m || interestRate > 100m)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InvalidPercent).WithData("value", interestRate);
        }

        InterestRate = interestRate;
        InterestPeriodDays = interestPeriodDays <= 0 ? 365 : interestPeriodDays;
        MinimumAmountLcy = Math.Max(0m, minimumAmountLcy);
        AdditionalFeeLcy = Math.Max(0m, additionalFeeLcy);
        GracePeriod = ReminderLevel.Formula(gracePeriod);
        DueDateCalculation = ReminderLevel.Formula(dueDateCalculation);
        PostInterest = postInterest;
        PostAdditionalFee = postAdditionalFee;
    }

    /// <summary>Interest on <paramref name="amount"/> overdue for <paramref name="days"/> days.</summary>
    public decimal Interest(decimal amount, int days) =>
        days <= 0 ? 0m : Math.Round(amount * InterestRate / 100m * days / InterestPeriodDays, 2, MidpointRounding.AwayFromZero);
}

/// <summary>
/// What a reminder and a finance charge memo share: a customer, a date, lines for the overdue
/// entries with the interest on them, and fee lines. Issuing posts the interest and fees.
/// </summary>
public abstract class CollectionDocument : CompanyAggregateRoot
{
    public string No { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerNo { get; private set; }
    public string CustomerName { get; private set; }
    public DateTime PostingDate { get; private set; }
    public DateTime DocumentDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public CollectionDocumentStatus Status { get; private set; }
    public string IssuedNo { get; private set; }
    public DateTime? IssuedDate { get; private set; }

    protected CollectionDocument() { }

    protected CollectionDocument(Guid id, string no, Customer customer, DateTime postingDate, DateTime documentDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxDocumentNoLength);
        CustomerId = customer.Id;
        CustomerNo = customer.No;
        CustomerName = customer.Name;
        SetDates(postingDate, documentDate, null);
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public abstract IEnumerable<CollectionDocumentLine> CollectionLines { get; }

    public void SetDates(DateTime postingDate, DateTime documentDate, DateTime? dueDate)
    {
        EnsureOpen();
        PostingDate = postingDate.Date;
        DocumentDate = documentDate.Date;
        DueDate = dueDate?.Date;
    }

    public void EnsureOpen()
    {
        if (Status == CollectionDocumentStatus.Issued)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.CollectionDocumentIssued).WithData("documentNo", No);
        }
    }

    internal void MarkIssued(string issuedNo, DateTime issuedDate)
    {
        Status = CollectionDocumentStatus.Issued;
        IssuedNo = issuedNo;
        IssuedDate = issuedDate.Date;
    }

    /// <summary>Interest plus fees: what issuing can post.</summary>
    public decimal TotalAmount => CollectionLines.Sum(l => l.Amount);

    public decimal RemainingAmount => CollectionLines.Where(l => l.Type == CollectionLineType.CustomerLedgerEntry).Sum(l => l.RemainingAmount);
}

/// <summary>A line of a reminder or finance charge memo.</summary>
public abstract class CollectionDocumentLine : Entity<Guid>
{
    public int LineNo { get; private set; }
    public CollectionLineType Type { get; private set; }

    /// <summary>The overdue customer entry, on an entry line.</summary>
    public long EntryNo { get; private set; }

    public string DocumentType { get; private set; }
    public string DocumentNo { get; private set; }
    public DateTime? DocumentDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public decimal OriginalAmount { get; private set; }
    public decimal RemainingAmount { get; private set; }

    /// <summary>The G/L account of a fee line.</summary>
    public string AccountNo { get; private set; }

    public string Description { get; private set; }

    /// <summary>Interest on an entry line, the fee on a fee line.</summary>
    public decimal Amount { get; private set; }

    /// <summary>The days the interest was calculated for.</summary>
    public int OverdueDays { get; private set; }

    protected CollectionDocumentLine() { }

    protected CollectionDocumentLine(Guid id, int lineNo, CollectionLineType type, string description)
        : base(id)
    {
        LineNo = lineNo;
        Type = type;
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    internal void SetEntry(CustomerLedgerEntry entry, decimal interest, int overdueDays)
    {
        EntryNo = entry.EntryNo;
        DocumentType = entry.DocumentType;
        DocumentNo = entry.DocumentNo;
        DocumentDate = entry.PostingDate;
        DueDate = entry.DueDate;
        OriginalAmount = entry.AmountLcy;
        RemainingAmount = entry.RemainingAmountLcy;
        Amount = interest;
        OverdueDays = overdueDays;
    }

    public void SetFee(string accountNo, decimal amount)
    {
        AccountNo = Check.Length(accountNo?.Trim(), nameof(accountNo), ErpDomainConsts.MaxNoLength);
        Amount = amount;
    }

    public void SetDescription(string description) =>
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
}

/// <summary>Reminder: tells a customer what is overdue, at the next level of their reminder terms.</summary>
public class ReminderHeader : CollectionDocument
{
    public string ReminderTermsCode { get; private set; }
    public int ReminderLevel { get; private set; }

    public List<ReminderLine> Lines { get; private set; } = new();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public override IEnumerable<CollectionDocumentLine> CollectionLines => Lines;

    protected ReminderHeader() { }

    public ReminderHeader(Guid id, string no, Customer customer, DateTime postingDate, DateTime documentDate, string reminderTermsCode, int reminderLevel)
        : base(id, no, customer, postingDate, documentDate)
    {
        ReminderTermsCode = reminderTermsCode;
        ReminderLevel = reminderLevel;
    }

    internal void SetLevel(int level) => ReminderLevel = level;
}

public class ReminderLine : CollectionDocumentLine
{
    public Guid ReminderHeaderId { get; private set; }

    protected ReminderLine() { }

    public ReminderLine(Guid id, Guid reminderHeaderId, int lineNo, CollectionLineType type, string description)
        : base(id, lineNo, type, description)
    {
        ReminderHeaderId = reminderHeaderId;
    }
}

/// <summary>Finance Charge Memo: the interest and fee charged on what a customer paid late or still owes.</summary>
public class FinanceChargeMemoHeader : CollectionDocument
{
    public string FinChargeTermsCode { get; private set; }

    public List<FinanceChargeMemoLine> Lines { get; private set; } = new();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public override IEnumerable<CollectionDocumentLine> CollectionLines => Lines;

    protected FinanceChargeMemoHeader() { }

    public FinanceChargeMemoHeader(Guid id, string no, Customer customer, DateTime postingDate, DateTime documentDate, string finChargeTermsCode)
        : base(id, no, customer, postingDate, documentDate)
    {
        FinChargeTermsCode = finChargeTermsCode;
    }
}

public class FinanceChargeMemoLine : CollectionDocumentLine
{
    public Guid FinanceChargeMemoHeaderId { get; private set; }

    protected FinanceChargeMemoLine() { }

    public FinanceChargeMemoLine(Guid id, Guid financeChargeMemoHeaderId, int lineNo, CollectionLineType type, string description)
        : base(id, lineNo, type, description)
    {
        FinanceChargeMemoHeaderId = financeChargeMemoHeaderId;
    }
}
