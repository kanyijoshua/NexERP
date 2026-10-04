using System;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;

namespace ABPmicroservice.Erp.CashManagement;

/// <summary>Bank Acc. Reconciliation.</summary>
public class BankAccReconciliation : CompanyEntity
{
    public BankAccRecStmtType StatementType { get; private set; }
    public string BankAccountNo { get; private set; }
    public string StatementNo { get; private set; }

    public decimal StatementEndingBalance { get; private set; }
    public DateTime? StatementDate { get; private set; }
    public decimal BalanceLastStatement { get; private set; }
    public string ShortcutDimension1Code { get; private set; }
    public string ShortcutDimension2Code { get; private set; }
    public bool PostPaymentsOnly { get; private set; }

    protected BankAccReconciliation() { }

    public BankAccReconciliation(Guid id, BankAccRecStmtType statementType, string bankAccountNo, string statementNo)
        : base(id)
    {
        SetKey(statementType, bankAccountNo, statementNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(BankAccRecStmtType statementType, string bankAccountNo, string statementNo)
    {
        StatementType = statementType;
        BankAccountNo = Check.NotNullOrWhiteSpace(bankAccountNo, nameof(bankAccountNo), 20).Trim().ToUpperInvariant();
        StatementNo = Check.NotNullOrWhiteSpace(statementNo, nameof(statementNo), 20).Trim().ToUpperInvariant();
    }

    public void Set(
        decimal statementEndingBalance,
        DateTime? statementDate,
        decimal balanceLastStatement,
        string shortcutDimension1Code,
        string shortcutDimension2Code,
        bool postPaymentsOnly
    )
    {
        StatementEndingBalance = statementEndingBalance;
        StatementDate = statementDate?.Date;
        BalanceLastStatement = balanceLastStatement;
        ShortcutDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(shortcutDimension1Code, nameof(shortcutDimension1Code), 20));
        ShortcutDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(shortcutDimension2Code, nameof(shortcutDimension2Code), 20));
        PostPaymentsOnly = postPaymentsOnly;
    }
}

/// <summary>Bank Acc. Reconciliation Line.</summary>
public class BankAccReconciliationLine : CompanyEntity
{
    public BankAccRecStmtType StatementType { get; private set; }
    public string BankAccountNo { get; private set; }
    public string StatementNo { get; private set; }
    public int StatementLineNo { get; private set; }

    public string DocumentNo { get; private set; }
    public DateTime? TransactionDate { get; private set; }
    public string Description { get; private set; }
    public decimal StatementAmount { get; private set; }
    public decimal Difference { get; private set; }
    public decimal AppliedAmount { get; private set; }
    public DateTime? ValueDate { get; private set; }
    public bool ReadyForApplication { get; private set; }
    public string CheckNo { get; private set; }
    public string RelatedPartyName { get; private set; }
    public string AdditionalTransactionInfo { get; private set; }
    public GenJournalAccountType AccountType { get; private set; }
    public string AccountNo { get; private set; }
    public string TransactionText { get; private set; }
    public string RelatedPartyBankAccNo { get; private set; }
    public string RelatedPartyAddress { get; private set; }
    public string RelatedPartyCity { get; private set; }
    public string PaymentReferenceNo { get; private set; }
    public string ShortcutDimension1Code { get; private set; }
    public string ShortcutDimension2Code { get; private set; }
    public string TransactionId { get; private set; }

    protected BankAccReconciliationLine() { }

    public BankAccReconciliationLine(Guid id, BankAccRecStmtType statementType, string bankAccountNo, string statementNo, int statementLineNo)
        : base(id)
    {
        SetKey(statementType, bankAccountNo, statementNo, statementLineNo);
    }

    /// <summary>The primary key, which is what other tables point at.</summary>
    public void SetKey(
        BankAccRecStmtType statementType,
        string bankAccountNo,
        string statementNo,
        int statementLineNo
    )
    {
        StatementType = statementType;
        BankAccountNo = Check.NotNullOrWhiteSpace(bankAccountNo, nameof(bankAccountNo), 20).Trim().ToUpperInvariant();
        StatementNo = Check.NotNullOrWhiteSpace(statementNo, nameof(statementNo), 20).Trim().ToUpperInvariant();
        StatementLineNo = statementLineNo;
    }

    public void Set(
        string documentNo,
        DateTime? transactionDate,
        string description,
        decimal statementAmount,
        decimal difference,
        decimal appliedAmount,
        DateTime? valueDate,
        bool readyForApplication,
        string checkNo,
        string relatedPartyName,
        string additionalTransactionInfo,
        GenJournalAccountType accountType,
        string accountNo,
        string transactionText,
        string relatedPartyBankAccNo,
        string relatedPartyAddress,
        string relatedPartyCity,
        string paymentReferenceNo,
        string shortcutDimension1Code,
        string shortcutDimension2Code,
        string transactionId
    )
    {
        DocumentNo = CodeTableEntity.NormalizeCode(Check.Length(documentNo, nameof(documentNo), 20));
        TransactionDate = transactionDate?.Date;
        Description = Check.Length(description, nameof(description), 100);
        StatementAmount = statementAmount;
        Difference = difference;
        AppliedAmount = appliedAmount;
        ValueDate = valueDate?.Date;
        ReadyForApplication = readyForApplication;
        CheckNo = CodeTableEntity.NormalizeCode(Check.Length(checkNo, nameof(checkNo), 20));
        RelatedPartyName = Check.Length(relatedPartyName, nameof(relatedPartyName), 250);
        AdditionalTransactionInfo = Check.Length(additionalTransactionInfo, nameof(additionalTransactionInfo), 100);
        AccountType = accountType;
        AccountNo = CodeTableEntity.NormalizeCode(Check.Length(accountNo, nameof(accountNo), 20));
        TransactionText = Check.Length(transactionText, nameof(transactionText), 140);
        RelatedPartyBankAccNo = Check.Length(relatedPartyBankAccNo, nameof(relatedPartyBankAccNo), 100);
        RelatedPartyAddress = Check.Length(relatedPartyAddress, nameof(relatedPartyAddress), 100);
        RelatedPartyCity = Check.Length(relatedPartyCity, nameof(relatedPartyCity), 50);
        PaymentReferenceNo = CodeTableEntity.NormalizeCode(Check.Length(paymentReferenceNo, nameof(paymentReferenceNo), 50));
        ShortcutDimension1Code = CodeTableEntity.NormalizeCode(Check.Length(shortcutDimension1Code, nameof(shortcutDimension1Code), 20));
        ShortcutDimension2Code = CodeTableEntity.NormalizeCode(Check.Length(shortcutDimension2Code, nameof(shortcutDimension2Code), 20));
        TransactionId = Check.Length(transactionId, nameof(transactionId), 50);
    }
}

/// <summary>Bank Account Statement.</summary>
public class BankAccountStatement : CompanyBasicEntity
{
    public string BankAccountNo { get; internal set; }
    public string StatementNo { get; internal set; }
    public decimal StatementEndingBalance { get; internal set; }
    public DateTime? StatementDate { get; internal set; }
    public decimal BalanceLastStatement { get; internal set; }
    public decimal GLBalanceAtPostingDate { get; internal set; }
    public decimal OutstdPaymentsAtPosting { get; internal set; }
    public decimal OutstdTransactAtPosting { get; internal set; }
    public decimal TotalPosDiffAtPosting { get; internal set; }
    public decimal TotalNegDiffAtPosting { get; internal set; }

    protected BankAccountStatement() { }

    public BankAccountStatement(Guid id)
        : base(id) { }
}

/// <summary>Bank Account Statement Line.</summary>
public class BankAccountStatementLine : CompanyBasicEntity
{
    public string BankAccountNo { get; internal set; }
    public string StatementNo { get; internal set; }
    public int StatementLineNo { get; internal set; }
    public string DocumentNo { get; internal set; }
    public DateTime? TransactionDate { get; internal set; }
    public string Description { get; internal set; }
    public decimal StatementAmount { get; internal set; }
    public decimal Difference { get; internal set; }
    public decimal AppliedAmount { get; internal set; }
    public BankAccStatementLineType Type { get; internal set; }
    public int AppliedEntries { get; internal set; }
    public DateTime? ValueDate { get; internal set; }
    public string CheckNo { get; internal set; }
    public string TransactionId { get; internal set; }

    protected BankAccountStatementLine() { }

    public BankAccountStatementLine(Guid id)
        : base(id) { }
}

/// <summary>Check Ledger Entry.</summary>
public class CheckLedgerEntry : LedgerEntryBase
{
    public string BankAccountNo { get; internal set; }
    public long BankAccountLedgerEntryNo { get; internal set; }
    public DateTime? PostingDate { get; internal set; }
    public GLEntryDocumentType DocumentType { get; internal set; }
    public string DocumentNo { get; internal set; }
    public string Description { get; internal set; }
    public decimal Amount { get; internal set; }
    public DateTime? CheckDate { get; internal set; }
    public string CheckNo { get; internal set; }
    public CheckLedgerEntryCheckType CheckType { get; internal set; }
    public BankPaymentType BankPaymentType { get; internal set; }
    public CheckLedgerEntryEntryStatus EntryStatus { get; internal set; }
    public CheckLedgerEntryOriginalEntryStatus OriginalEntryStatus { get; internal set; }
    public GenJournalAccountType BalAccountType { get; internal set; }
    public string BalAccountNo { get; internal set; }
    public bool Open { get; internal set; }
    public CheckLedgerEntryStatementStatus StatementStatus { get; internal set; }
    public string StatementNo { get; internal set; }
    public int StatementLineNo { get; internal set; }
    public string UserId { get; internal set; }
    public string ExternalDocumentNo { get; internal set; }

    protected CheckLedgerEntry() { }

    public CheckLedgerEntry(Guid id)
        : base(id) { }
}
