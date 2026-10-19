using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Finance;

// --- applying entries --------------------------------------------------------------------------

public class GetApplicableEntriesInput
{
    public PartyLedgerType PartyType { get; set; }

    /// <summary>The entry being applied; the result is the open entries it can be applied to.</summary>
    public Guid ApplyingEntryId { get; set; }
}

public class EntryToApplyDto
{
    public Guid EntryId { get; set; }

    /// <summary>Blank applies everything open on the entry.</summary>
    public decimal? AmountToApply { get; set; }
}

public class ApplyEntriesInput
{
    public PartyLedgerType PartyType { get; set; }
    public Guid ApplyingEntryId { get; set; }
    public DateTime PostingDate { get; set; }
    public List<EntryToApplyDto> Entries { get; set; } = new();
}

public class UnapplyEntryInput
{
    public PartyLedgerType PartyType { get; set; }
    public Guid EntryId { get; set; }
    public DateTime PostingDate { get; set; }
}

public class ApplicationResultDto
{
    public long ApplicationNo { get; set; }
    public PartyLedgerEntryDto ApplyingEntry { get; set; }
}

public class GetDetailedEntriesInput
{
    public PartyLedgerType PartyType { get; set; }
    public long LedgerEntryNo { get; set; }
}

/// <summary>A detailed customer or vendor ledger entry: one movement of an entry's open amount.</summary>
public class DetailedLedgerEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public long LedgerEntryNo { get; set; }
    public DetailedCVLedgerEntryType EntryType { get; set; }
    public DateTime? PostingDate { get; set; }
    public GLEntryDocumentType DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountLcy { get; set; }
    public long AppliedEntryNo { get; set; }
    public int ApplicationNo { get; set; }
    public bool Unapplied { get; set; }
}

public interface IEntryApplicationAppService : IApplicationService
{
    Task<List<PartyLedgerEntryDto>> GetApplicableEntriesAsync(GetApplicableEntriesInput input);

    Task<ApplicationResultDto> ApplyAsync(ApplyEntriesInput input);

    Task<int> UnapplyAsync(UnapplyEntryInput input);

    Task<List<DetailedLedgerEntryDto>> GetDetailedEntriesAsync(GetDetailedEntriesInput input);
}

// --- bank reconciliation -----------------------------------------------------------------------

public class BankRecLineDto : EntityDto<Guid>
{
    public int StatementLineNo { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public decimal StatementAmount { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal Difference { get; set; }
    public GenJournalAccountType AccountType { get; set; }
    public string AccountNo { get; set; }
}

public class BankRecEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public DateTime PostingDate { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public BankEntryStatementStatus StatementStatus { get; set; }
    public int StatementLineNo { get; set; }
}

/// <summary>Everything the reconciliation page shows: the statement, its lines and the entries to match.</summary>
public class BankRecWorksheetDto
{
    public Guid Id { get; set; }
    public string BankAccountNo { get; set; }
    public string StatementNo { get; set; }
    public DateTime? StatementDate { get; set; }
    public decimal BalanceLastStatement { get; set; }
    public decimal StatementEndingBalance { get; set; }
    public decimal TotalStatementAmount { get; set; }

    /// <summary>Ending balance less last balance less the lines; must be zero to post.</summary>
    public decimal Difference { get; set; }

    public List<BankRecLineDto> Lines { get; set; } = new();
    public List<BankRecEntryDto> Entries { get; set; } = new();
}

public class ImportBankStatementInput
{
    /// <summary>The statement file as text: date, description, amount, document no. per row.</summary>
    [Required]
    public string Content { get; set; }
}

public class MatchAutomaticallyInput
{
    [Range(0, 90)]
    public int DateTolerance { get; set; }
}

public class MatchManuallyInput
{
    public Guid LineId { get; set; }
    public List<Guid> EntryIds { get; set; } = new();
}

public class RemoveMatchInput
{
    public Guid LineId { get; set; }
}

public interface IBankReconciliationAppService : IApplicationService
{
    Task<BankRecWorksheetDto> GetWorksheetAsync(Guid id);

    Task<int> ImportStatementAsync(Guid id, ImportBankStatementInput input);

    Task<int> MatchAutomaticallyAsync(Guid id, MatchAutomaticallyInput input);

    Task MatchManuallyAsync(Guid id, MatchManuallyInput input);

    Task UnmatchAsync(Guid id, RemoveMatchInput input);

    /// <summary>Posts the differences of lines that name an account, and matches what they post.</summary>
    Task<int> TransferDifferencesAsync(Guid id);

    /// <summary>Posts the statement. Returns the statement no.</summary>
    Task<string> RunPostingAsync(Guid id);
}

// --- reminders and finance charge memos --------------------------------------------------------

public class ReminderTermsDto : CodeTableDto
{
    public bool PostInterest { get; set; }
    public bool PostAdditionalFee { get; set; }
    public int MaxNoOfReminders { get; set; }
    public decimal MinimumAmountLcy { get; set; }
}

public class CreateUpdateReminderTermsDto : CreateUpdateCodeTableDto
{
    public bool PostInterest { get; set; }
    public bool PostAdditionalFee { get; set; }
    public int MaxNoOfReminders { get; set; }
    public decimal MinimumAmountLcy { get; set; }
}

public class ReminderLevelDto : EntityDto<Guid>
{
    public string ReminderTermsCode { get; set; }
    public int No { get; set; }
    public string GracePeriod { get; set; }
    public string DueDateCalculation { get; set; }
    public decimal AdditionalFeeLcy { get; set; }
    public bool CalculateInterest { get; set; }
}

public class CreateUpdateReminderLevelDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxReminderTermsCodeLength)]
    public string ReminderTermsCode { get; set; }

    public int No { get; set; }

    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string GracePeriod { get; set; }

    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string DueDateCalculation { get; set; }

    public decimal AdditionalFeeLcy { get; set; }
    public bool CalculateInterest { get; set; }
}

public class GetReminderLevelListInput : ErpPagedListInput
{
    public string ReminderTermsCode { get; set; }
}

public class FinanceChargeTermsDto : CodeTableDto
{
    public decimal InterestRate { get; set; }
    public int InterestPeriodDays { get; set; }
    public decimal MinimumAmountLcy { get; set; }
    public decimal AdditionalFeeLcy { get; set; }
    public string GracePeriod { get; set; }
    public string DueDateCalculation { get; set; }
    public bool PostInterest { get; set; }
    public bool PostAdditionalFee { get; set; }
}

public class CreateUpdateFinanceChargeTermsDto : CreateUpdateCodeTableDto
{
    public decimal InterestRate { get; set; }
    public int InterestPeriodDays { get; set; } = 365;
    public decimal MinimumAmountLcy { get; set; }
    public decimal AdditionalFeeLcy { get; set; }

    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string GracePeriod { get; set; }

    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string DueDateCalculation { get; set; }

    public bool PostInterest { get; set; } = true;
    public bool PostAdditionalFee { get; set; } = true;
}

public class CollectionDocumentLineDto : EntityDto<Guid>
{
    public int LineNo { get; set; }
    public CollectionLineType Type { get; set; }
    public long EntryNo { get; set; }
    public string DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public DateTime? DocumentDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string AccountNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public int OverdueDays { get; set; }
}

/// <summary>A reminder or a finance charge memo.</summary>
public class CollectionDocumentDto : EntityDto<Guid>
{
    public string No { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerNo { get; set; }
    public string CustomerName { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime DocumentDate { get; set; }
    public DateTime? DueDate { get; set; }
    public CollectionDocumentStatus Status { get; set; }
    public string IssuedNo { get; set; }
    public DateTime? IssuedDate { get; set; }

    /// <summary>The terms: reminder terms on a reminder, finance charge terms on a memo.</summary>
    public string TermsCode { get; set; }

    /// <summary>The reminder level; zero on a finance charge memo.</summary>
    public int ReminderLevel { get; set; }

    public decimal RemainingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<CollectionDocumentLineDto> Lines { get; set; } = new();
}

public class UpdateCollectionDocumentDto
{
    public DateTime PostingDate { get; set; }
    public DateTime DocumentDate { get; set; }
    public DateTime? DueDate { get; set; }
}

public class GetCollectionDocumentListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string CustomerNo { get; set; }
    public CollectionDocumentStatus? Status { get; set; }
}

public class SuggestCollectionDocumentsInput
{
    public DateTime DocumentDate { get; set; }

    /// <summary>Blank creates one for every customer that has something due.</summary>
    public string CustomerNo { get; set; }
}

public class IssueCollectionDocumentInput
{
    /// <summary>Blank posts on the document's posting date.</summary>
    public DateTime? PostingDate { get; set; }
}

/// <summary>Reminders and, with the same shape, finance charge memos.</summary>
public interface ICollectionDocumentAppService : IApplicationService
{
    Task<CollectionDocumentDto> GetAsync(Guid id);

    Task<PagedResultDto<CollectionDocumentDto>> GetListAsync(GetCollectionDocumentListInput input);

    Task<CollectionDocumentDto> UpdateAsync(Guid id, UpdateCollectionDocumentDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Creates a document for every customer with something due. Returns how many.</summary>
    Task<int> SuggestAsync(SuggestCollectionDocumentsInput input);

    Task<CollectionDocumentDto> IssueAsync(Guid id, IssueCollectionDocumentInput input);
}

public interface IReminderAppService : ICollectionDocumentAppService { }

public interface IFinanceChargeMemoAppService : ICollectionDocumentAppService { }

public interface IReminderTermsAppService : ICrudAppService<ReminderTermsDto, Guid, GetCodeTableListInput, CreateUpdateReminderTermsDto> { }

public interface IReminderLevelAppService : ICrudAppService<ReminderLevelDto, Guid, GetReminderLevelListInput, CreateUpdateReminderLevelDto> { }

public interface IFinanceChargeTermsAppService : ICrudAppService<FinanceChargeTermsDto, Guid, GetCodeTableListInput, CreateUpdateFinanceChargeTermsDto> { }

// --- deferrals ---------------------------------------------------------------------------------

public class DeferralTemplateDto : CodeTableDto
{
    public string DeferralAccountNo { get; set; }
    public decimal DeferralPct { get; set; }
    public DeferralCalculationMethod CalcMethod { get; set; }
    public DeferralStartDate StartDate { get; set; }
    public int NoOfPeriods { get; set; }
    public string PeriodDescription { get; set; }
}

public class CreateUpdateDeferralTemplateDto : CreateUpdateCodeTableDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string DeferralAccountNo { get; set; }

    public decimal DeferralPct { get; set; } = 100m;
    public DeferralCalculationMethod CalcMethod { get; set; }
    public DeferralStartDate StartDate { get; set; }

    [Range(1, ErpDomainConsts.MaxDeferralPeriods)]
    public int NoOfPeriods { get; set; } = 12;

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string PeriodDescription { get; set; }
}

public class PostedDeferralLineDto : EntityDto<Guid>
{
    public DateTime PostingDate { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
}

public class PostedDeferralHeaderDto : EntityDto<Guid>
{
    public DeferralSourceType SourceType { get; set; }
    public string DocumentNo { get; set; }
    public int LineNo { get; set; }
    public string DeferralCode { get; set; }
    public string AccountNo { get; set; }
    public string DeferralAccountNo { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime StartDate { get; set; }
    public int NoOfPeriods { get; set; }
    public decimal AmountToDefer { get; set; }
    public string Description { get; set; }
    public List<PostedDeferralLineDto> Lines { get; set; } = new();
}

public class GetPostedDeferralListInput : ErpPagedListInput
{
    public string Filter { get; set; }
}

public class PreviewDeferralInput
{
    [Required]
    public string DeferralCode { get; set; }

    public decimal Amount { get; set; }
    public DateTime PostingDate { get; set; }
}

public class DeferralPeriodDto
{
    public DateTime PostingDate { get; set; }
    public decimal Amount { get; set; }
}

public interface IDeferralTemplateAppService : ICrudAppService<DeferralTemplateDto, Guid, GetCodeTableListInput, CreateUpdateDeferralTemplateDto>
{
    Task<List<DeferralPeriodDto>> PreviewScheduleAsync(PreviewDeferralInput input);
}

public interface IPostedDeferralAppService : IReadOnlyAppService<PostedDeferralHeaderDto, Guid, GetPostedDeferralListInput> { }

// --- cash flow ---------------------------------------------------------------------------------

public class CashFlowForecastDto : EntityDto<Guid>
{
    public string No { get; set; }
    public string Description { get; set; }
    public DateTime? ManualPaymentsTo { get; set; }
    public bool ConsiderReceivables { get; set; }
    public bool ConsiderPayables { get; set; }
    public bool ConsiderSalesOrders { get; set; }
    public bool ConsiderPurchaseOrders { get; set; }
    public bool ConsiderManual { get; set; }
    public bool ConsiderLiquidFunds { get; set; }
    public string GLBudgetName { get; set; }
    public DateTime? GLBudgetFrom { get; set; }
    public DateTime? GLBudgetTo { get; set; }
}

public class CreateUpdateCashFlowForecastDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string No { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public DateTime? ManualPaymentsTo { get; set; }
    public bool ConsiderReceivables { get; set; } = true;
    public bool ConsiderPayables { get; set; } = true;
    public bool ConsiderSalesOrders { get; set; } = true;
    public bool ConsiderPurchaseOrders { get; set; } = true;
    public bool ConsiderManual { get; set; } = true;
    public bool ConsiderLiquidFunds { get; set; } = true;

    [StringLength(10)]
    public string GLBudgetName { get; set; }

    public DateTime? GLBudgetFrom { get; set; }
    public DateTime? GLBudgetTo { get; set; }
}

public class CashFlowManualEntryDto : CodeTableDto
{
    public CashFlowManualKind Kind { get; set; }
    public decimal Amount { get; set; }
    public DateTime StartingDate { get; set; }
    public string RecurringFrequency { get; set; }
    public DateTime? EndingDate { get; set; }
}

public class CreateUpdateCashFlowManualEntryDto : CreateUpdateCodeTableDto
{
    public CashFlowManualKind Kind { get; set; }
    public decimal Amount { get; set; }
    public DateTime StartingDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDateFormulaLength)]
    public string RecurringFrequency { get; set; }

    public DateTime? EndingDate { get; set; }
}

public class CashFlowWorksheetLineDto : EntityDto<Guid>
{
    public string CashFlowForecastNo { get; set; }
    public int LineNo { get; set; }
    public DateTime CashFlowDate { get; set; }
    public CashFlowSourceType SourceType { get; set; }
    public string SourceNo { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
}

public class CreateUpdateCashFlowWorksheetLineDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string CashFlowForecastNo { get; set; }

    public DateTime CashFlowDate { get; set; }
    public CashFlowSourceType SourceType { get; set; } = CashFlowSourceType.Manual;

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string SourceNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string DocumentNo { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public decimal Amount { get; set; }
}

public class GetCashFlowLineListInput : ErpPagedListInput
{
    public string CashFlowForecastNo { get; set; }
    public string Filter { get; set; }
}

public class CashFlowForecastEntryDto : EntityDto<Guid>
{
    public long EntryNo { get; set; }
    public string CashFlowForecastNo { get; set; }
    public DateTime CashFlowDate { get; set; }
    public CashFlowSourceType SourceType { get; set; }
    public string SourceNo { get; set; }
    public string DocumentNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
}

public class SuggestCashFlowInput
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class AnalysisMatrixInput
{
    public DateTime StartDate { get; set; }
    public AnalysisPeriodType PeriodType { get; set; } = AnalysisPeriodType.Month;

    [Range(1, ErpDomainConsts.MaxAnalysisPeriods)]
    public int PeriodCount { get; set; } = 12;
}

public class AnalysisPeriodDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Name { get; set; }
}

public class CashFlowPeriodRowDto
{
    public CashFlowSourceType SourceType { get; set; }
    public List<decimal> Amounts { get; set; } = new();
    public decimal Total { get; set; }
}

public class CashFlowByPeriodDto
{
    public List<AnalysisPeriodDto> Periods { get; set; } = new();
    public List<CashFlowPeriodRowDto> Rows { get; set; } = new();

    /// <summary>The net flow of each period.</summary>
    public List<decimal> Totals { get; set; } = new();

    /// <summary>The cash expected at the end of each period: the running sum of the totals.</summary>
    public List<decimal> Balances { get; set; } = new();
}

public interface ICashFlowForecastAppService : ICrudAppService<CashFlowForecastDto, Guid, GetCodeTableListInput, CreateUpdateCashFlowForecastDto>
{
    Task<int> SuggestWorksheetLinesAsync(Guid id, SuggestCashFlowInput input);

    Task<int> RegisterAsync(Guid id);

    Task<CashFlowByPeriodDto> GetByPeriodAsync(Guid id, AnalysisMatrixInput input);
}

public interface ICashFlowManualEntryAppService : ICrudAppService<CashFlowManualEntryDto, Guid, GetCodeTableListInput, CreateUpdateCashFlowManualEntryDto> { }

public interface ICashFlowWorksheetLineAppService : ICrudAppService<CashFlowWorksheetLineDto, Guid, GetCashFlowLineListInput, CreateUpdateCashFlowWorksheetLineDto> { }

public interface ICashFlowForecastEntryAppService : IReadOnlyAppService<CashFlowForecastEntryDto, Guid, GetCashFlowLineListInput> { }

// --- budgets -----------------------------------------------------------------------------------

public class BudgetMatrixInput : AnalysisMatrixInput
{
    [Required]
    public string BudgetName { get; set; }

    /// <summary>Accounts whose number starts with this.</summary>
    public string AccountFilter { get; set; }

    public bool WithActual { get; set; }

    public bool IncomeStatementOnly { get; set; }
}

public class BudgetMatrixRowDto
{
    public string AccountNo { get; set; }
    public string AccountName { get; set; }
    public GLAccountType AccountType { get; set; }
    public int Indentation { get; set; }
    public List<decimal> Budget { get; set; } = new();
    public List<decimal> Actual { get; set; } = new();
    public decimal TotalBudget { get; set; }
    public decimal TotalActual { get; set; }
}

public class BudgetMatrixDto
{
    public string BudgetName { get; set; }
    public List<AnalysisPeriodDto> Periods { get; set; } = new();
    public List<BudgetMatrixRowDto> Rows { get; set; } = new();
}

public class SetBudgetCellInput
{
    [Required]
    public string BudgetName { get; set; }

    [Required]
    public string AccountNo { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Amount { get; set; }
}

public class CopyBudgetInput
{
    [Required]
    public string FromBudgetName { get; set; }

    [Required]
    public string ToBudgetName { get; set; }

    /// <summary>A date formula moving every entry, such as "1Y".</summary>
    public string DateChange { get; set; }

    /// <summary>Multiplies every amount; blank or zero keeps them.</summary>
    public decimal Factor { get; set; } = 1m;

    public bool Replace { get; set; }
}

public interface IBudgetMatrixAppService : IApplicationService
{
    Task<BudgetMatrixDto> GetMatrixAsync(BudgetMatrixInput input);

    Task SetCellAsync(SetBudgetCellInput input);

    Task<int> CopyBudgetAsync(CopyBudgetInput input);
}
