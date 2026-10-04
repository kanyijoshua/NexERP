using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.CashManagement;

public class BankAccReconciliationDto : FullAuditedEntityDto<Guid>
{
    public BankAccRecStmtType StatementType { get; set; }
    public string BankAccountNo { get; set; }
    public string StatementNo { get; set; }
    public decimal StatementEndingBalance { get; set; }
    public DateTime? StatementDate { get; set; }
    public decimal BalanceLastStatement { get; set; }
    public string ShortcutDimension1Code { get; set; }
    public string ShortcutDimension2Code { get; set; }
    public bool PostPaymentsOnly { get; set; }
}

public class CreateUpdateBankAccReconciliationDto
{
    public BankAccRecStmtType StatementType { get; set; }

    [Required]
    [StringLength(20)]
    public string BankAccountNo { get; set; }

    [Required]
    [StringLength(20)]
    public string StatementNo { get; set; }

    public decimal StatementEndingBalance { get; set; }

    public DateTime? StatementDate { get; set; }

    public decimal BalanceLastStatement { get; set; }

    [StringLength(20)]
    public string ShortcutDimension1Code { get; set; }

    [StringLength(20)]
    public string ShortcutDimension2Code { get; set; }

    public bool PostPaymentsOnly { get; set; }
}

public class GetBankAccReconciliationListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string BankAccountNo { get; set; }
}

/// <summary>Bank Acc. Reconciliations.</summary>
public interface IBankAccReconciliationAppService : ICrudAppService<BankAccReconciliationDto, Guid, GetBankAccReconciliationListInput, CreateUpdateBankAccReconciliationDto, CreateUpdateBankAccReconciliationDto> { }

public class BankAccReconciliationLineDto : FullAuditedEntityDto<Guid>
{
    public BankAccRecStmtType StatementType { get; set; }
    public string BankAccountNo { get; set; }
    public string StatementNo { get; set; }
    public int StatementLineNo { get; set; }
    public string DocumentNo { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string Description { get; set; }
    public decimal StatementAmount { get; set; }
    public decimal Difference { get; set; }
    public decimal AppliedAmount { get; set; }
    public DateTime? ValueDate { get; set; }
    public bool ReadyForApplication { get; set; }
    public string CheckNo { get; set; }
    public string RelatedPartyName { get; set; }
    public string AdditionalTransactionInfo { get; set; }
    public GenJournalAccountType AccountType { get; set; }
    public string AccountNo { get; set; }
    public string TransactionText { get; set; }
    public string RelatedPartyBankAccNo { get; set; }
    public string RelatedPartyAddress { get; set; }
    public string RelatedPartyCity { get; set; }
    public string PaymentReferenceNo { get; set; }
    public string ShortcutDimension1Code { get; set; }
    public string ShortcutDimension2Code { get; set; }
    public string TransactionId { get; set; }
}

public class CreateUpdateBankAccReconciliationLineDto
{
    public BankAccRecStmtType StatementType { get; set; }

    [Required]
    [StringLength(20)]
    public string BankAccountNo { get; set; }

    [Required]
    [StringLength(20)]
    public string StatementNo { get; set; }

    /// <summary>Zero takes the next free number.</summary>
    public int StatementLineNo { get; set; }

    [StringLength(20)]
    public string DocumentNo { get; set; }

    public DateTime? TransactionDate { get; set; }

    [StringLength(100)]
    public string Description { get; set; }

    public decimal StatementAmount { get; set; }

    public decimal Difference { get; set; }

    public decimal AppliedAmount { get; set; }

    public DateTime? ValueDate { get; set; }

    public bool ReadyForApplication { get; set; }

    [StringLength(20)]
    public string CheckNo { get; set; }

    [StringLength(250)]
    public string RelatedPartyName { get; set; }

    [StringLength(100)]
    public string AdditionalTransactionInfo { get; set; }

    public GenJournalAccountType AccountType { get; set; }

    [StringLength(20)]
    public string AccountNo { get; set; }

    [StringLength(140)]
    public string TransactionText { get; set; }

    [StringLength(100)]
    public string RelatedPartyBankAccNo { get; set; }

    [StringLength(100)]
    public string RelatedPartyAddress { get; set; }

    [StringLength(50)]
    public string RelatedPartyCity { get; set; }

    [StringLength(50)]
    public string PaymentReferenceNo { get; set; }

    [StringLength(20)]
    public string ShortcutDimension1Code { get; set; }

    [StringLength(20)]
    public string ShortcutDimension2Code { get; set; }

    [StringLength(50)]
    public string TransactionId { get; set; }
}

public class GetBankAccReconciliationLineListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string BankAccountNo { get; set; }
    public string StatementNo { get; set; }
}

/// <summary>Bank Acc. Reconciliation Lines.</summary>
public interface IBankAccReconciliationLineAppService : ICrudAppService<BankAccReconciliationLineDto, Guid, GetBankAccReconciliationLineListInput, CreateUpdateBankAccReconciliationLineDto, CreateUpdateBankAccReconciliationLineDto> { }

public class BankAccountStatementDto : EntityDto<Guid>
{
    public string BankAccountNo { get; set; }
    public string StatementNo { get; set; }
    public decimal StatementEndingBalance { get; set; }
    public DateTime? StatementDate { get; set; }
    public decimal BalanceLastStatement { get; set; }
    public decimal GLBalanceAtPostingDate { get; set; }
    public decimal OutstdPaymentsAtPosting { get; set; }
    public decimal OutstdTransactAtPosting { get; set; }
    public decimal TotalPosDiffAtPosting { get; set; }
    public decimal TotalNegDiffAtPosting { get; set; }
}

public class GetBankAccountStatementListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string BankAccountNo { get; set; }
}

/// <summary>Bank Account Statements.</summary>
public interface IBankAccountStatementAppService : IReadOnlyAppService<BankAccountStatementDto, Guid, GetBankAccountStatementListInput> { }

public class BankAccountStatementLineDto : EntityDto<Guid>
{
    public string BankAccountNo { get; set; }
    public string StatementNo { get; set; }
    public int StatementLineNo { get; set; }
    public string DocumentNo { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string Description { get; set; }
    public decimal StatementAmount { get; set; }
    public decimal Difference { get; set; }
    public decimal AppliedAmount { get; set; }
    public BankAccStatementLineType Type { get; set; }
    public int AppliedEntries { get; set; }
    public DateTime? ValueDate { get; set; }
    public string CheckNo { get; set; }
    public string TransactionId { get; set; }
}

public class GetBankAccountStatementLineListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string BankAccountNo { get; set; }
    public string StatementNo { get; set; }
}

/// <summary>Bank Account Statement Lines.</summary>
public interface IBankAccountStatementLineAppService : IReadOnlyAppService<BankAccountStatementLineDto, Guid, GetBankAccountStatementLineListInput> { }

public class CheckLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public string BankAccountNo { get; set; }
    public long BankAccountLedgerEntryNo { get; set; }
    public DateTime? PostingDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime? CheckDate { get; set; }
    public string CheckNo { get; set; }
    public CheckLedgerEntryCheckType CheckType { get; set; }
    public BankPaymentType BankPaymentType { get; set; }
    public CheckLedgerEntryEntryStatus EntryStatus { get; set; }
    public CheckLedgerEntryOriginalEntryStatus OriginalEntryStatus { get; set; }
    public GenJournalAccountType BalAccountType { get; set; }
    public string BalAccountNo { get; set; }
    public bool Open { get; set; }
    public CheckLedgerEntryStatementStatus StatementStatus { get; set; }
    public string StatementNo { get; set; }
    public int StatementLineNo { get; set; }
    public string UserId { get; set; }
    public string ExternalDocumentNo { get; set; }
}

public class GetCheckLedgerEntryListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string BankAccountNo { get; set; }
    public string DocumentNo { get; set; }
}

/// <summary>Check Ledger Entries.</summary>
public interface ICheckLedgerEntryAppService : IReadOnlyAppService<CheckLedgerEntryDto, Guid, GetCheckLedgerEntryListInput> { }
