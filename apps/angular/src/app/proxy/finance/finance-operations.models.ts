import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { GenJournalAccountType } from './gen-journal-account-type.enum';
import type { GLAccountType } from './glaccount-type.enum';
import type { GLEntryDocumentType } from './glentry-document-type.enum';
import type { PartyLedgerEntryDto } from './periodic-activities.models';
import type {
  AnalysisPeriodType,
  BankEntryStatementStatus,
  CashFlowManualKind,
  CashFlowSourceType,
  CollectionDocumentStatus,
  CollectionLineType,
  DeferralCalculationMethod,
  DeferralSourceType,
  DeferralStartDate,
  PartyLedgerType,
} from './finance-operations.enums';
import type { DetailedCVLedgerEntryType } from '../sales/detailed-cv-ledger-entry-type.enum';

export interface GetApplicableEntriesInput {
  partyType: PartyLedgerType;
  applyingEntryId?: string;
}

export interface EntryToApplyDto {
  entryId?: string;
  amountToApply?: number;
}

export interface ApplyEntriesInput {
  partyType: PartyLedgerType;
  applyingEntryId?: string;
  postingDate?: string;
  entries: EntryToApplyDto[];
}

export interface UnapplyEntryInput {
  partyType: PartyLedgerType;
  entryId?: string;
  postingDate?: string;
}

export interface ApplicationResultDto {
  applicationNo: number;
  applyingEntry: PartyLedgerEntryDto;
}

export interface GetDetailedEntriesInput {
  partyType: PartyLedgerType;
  ledgerEntryNo: number;
}

export interface DetailedLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  ledgerEntryNo: number;
  entryType: DetailedCVLedgerEntryType;
  postingDate?: string;
  documentType: GLEntryDocumentType;
  documentNo?: string;
  amount: number;
  amountLcy: number;
  appliedEntryNo: number;
  applicationNo: number;
  unapplied: boolean;
}

export interface BankRecLineDto extends EntityDto<string> {
  statementLineNo: number;
  transactionDate?: string;
  documentNo?: string;
  description?: string;
  statementAmount: number;
  appliedAmount: number;
  difference: number;
  accountType: GenJournalAccountType;
  accountNo?: string;
}

export interface BankRecEntryDto extends EntityDto<string> {
  entryNo: number;
  postingDate?: string;
  documentNo?: string;
  description?: string;
  amount: number;
  statementStatus: BankEntryStatementStatus;
  statementLineNo: number;
}

export interface BankRecWorksheetDto {
  id?: string;
  bankAccountNo?: string;
  statementNo?: string;
  statementDate?: string;
  balanceLastStatement: number;
  statementEndingBalance: number;
  totalStatementAmount: number;
  difference: number;
  lines: BankRecLineDto[];
  entries: BankRecEntryDto[];
}

export interface ImportBankStatementInput {
  content: string;
}

export interface MatchAutomaticallyInput {
  dateTolerance: number;
}

export interface MatchManuallyInput {
  lineId?: string;
  entryIds: string[];
}

export interface RemoveMatchInput {
  lineId?: string;
}

export interface ReminderTermsDto extends CodeTableDto {
  postInterest: boolean;
  postAdditionalFee: boolean;
  maxNoOfReminders: number;
  minimumAmountLcy: number;
}

export interface CreateUpdateReminderTermsDto extends CreateUpdateCodeTableDto {
  postInterest: boolean;
  postAdditionalFee: boolean;
  maxNoOfReminders: number;
  minimumAmountLcy: number;
}

export interface ReminderLevelDto extends EntityDto<string> {
  reminderTermsCode?: string;
  no: number;
  gracePeriod?: string;
  dueDateCalculation?: string;
  additionalFeeLcy: number;
  calculateInterest: boolean;
}

export interface CreateUpdateReminderLevelDto {
  reminderTermsCode: string;
  no: number;
  gracePeriod?: string;
  dueDateCalculation?: string;
  additionalFeeLcy: number;
  calculateInterest: boolean;
}

export interface GetReminderLevelListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  reminderTermsCode?: string;
}

export interface FinanceChargeTermsDto extends CodeTableDto {
  interestRate: number;
  interestPeriodDays: number;
  minimumAmountLcy: number;
  additionalFeeLcy: number;
  gracePeriod?: string;
  dueDateCalculation?: string;
  postInterest: boolean;
  postAdditionalFee: boolean;
}

export interface CreateUpdateFinanceChargeTermsDto extends CreateUpdateCodeTableDto {
  interestRate: number;
  interestPeriodDays: number;
  minimumAmountLcy: number;
  additionalFeeLcy: number;
  gracePeriod?: string;
  dueDateCalculation?: string;
  postInterest: boolean;
  postAdditionalFee: boolean;
}

export interface CollectionDocumentLineDto extends EntityDto<string> {
  lineNo: number;
  type: CollectionLineType;
  entryNo: number;
  documentType?: string;
  documentNo?: string;
  documentDate?: string;
  dueDate?: string;
  originalAmount: number;
  remainingAmount: number;
  accountNo?: string;
  description?: string;
  amount: number;
  overdueDays: number;
}

export interface CollectionDocumentDto extends EntityDto<string> {
  no?: string;
  customerId?: string;
  customerNo?: string;
  customerName?: string;
  postingDate?: string;
  documentDate?: string;
  dueDate?: string;
  status: CollectionDocumentStatus;
  issuedNo?: string;
  issuedDate?: string;
  termsCode?: string;
  reminderLevel: number;
  remainingAmount: number;
  totalAmount: number;
  lines: CollectionDocumentLineDto[];
}

export interface UpdateCollectionDocumentDto {
  postingDate?: string;
  documentDate?: string;
  dueDate?: string;
}

export interface GetCollectionDocumentListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  customerNo?: string;
  status?: CollectionDocumentStatus;
}

export interface SuggestCollectionDocumentsInput {
  documentDate?: string;
  customerNo?: string;
}

export interface IssueCollectionDocumentInput {
  postingDate?: string;
}

export interface DeferralTemplateDto extends CodeTableDto {
  deferralAccountNo?: string;
  deferralPct: number;
  calcMethod: DeferralCalculationMethod;
  startDate: DeferralStartDate;
  noOfPeriods: number;
  periodDescription?: string;
}

export interface CreateUpdateDeferralTemplateDto extends CreateUpdateCodeTableDto {
  deferralAccountNo: string;
  deferralPct: number;
  calcMethod: DeferralCalculationMethod;
  startDate: DeferralStartDate;
  noOfPeriods: number;
  periodDescription?: string;
}

export interface PostedDeferralLineDto extends EntityDto<string> {
  postingDate?: string;
  description?: string;
  amount: number;
}

export interface PostedDeferralHeaderDto extends EntityDto<string> {
  sourceType: DeferralSourceType;
  documentNo?: string;
  lineNo: number;
  deferralCode?: string;
  accountNo?: string;
  deferralAccountNo?: string;
  postingDate?: string;
  startDate?: string;
  noOfPeriods: number;
  amountToDefer: number;
  description?: string;
  lines: PostedDeferralLineDto[];
}

export interface GetPostedDeferralListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
}

export interface PreviewDeferralInput {
  deferralCode: string;
  amount: number;
  postingDate?: string;
}

export interface DeferralPeriodDto {
  postingDate?: string;
  amount: number;
}

export interface CashFlowForecastDto extends EntityDto<string> {
  no?: string;
  description?: string;
  manualPaymentsTo?: string;
  considerReceivables: boolean;
  considerPayables: boolean;
  considerSalesOrders: boolean;
  considerPurchaseOrders: boolean;
  considerManual: boolean;
  considerLiquidFunds: boolean;
  glBudgetName?: string;
  glBudgetFrom?: string;
  glBudgetTo?: string;
}

export interface CreateUpdateCashFlowForecastDto {
  no: string;
  description?: string;
  manualPaymentsTo?: string;
  considerReceivables: boolean;
  considerPayables: boolean;
  considerSalesOrders: boolean;
  considerPurchaseOrders: boolean;
  considerManual: boolean;
  considerLiquidFunds: boolean;
  glBudgetName?: string;
  glBudgetFrom?: string;
  glBudgetTo?: string;
}

export interface CashFlowManualEntryDto extends CodeTableDto {
  kind: CashFlowManualKind;
  amount: number;
  startingDate?: string;
  recurringFrequency?: string;
  endingDate?: string;
}

export interface CreateUpdateCashFlowManualEntryDto extends CreateUpdateCodeTableDto {
  kind: CashFlowManualKind;
  amount: number;
  startingDate?: string;
  recurringFrequency?: string;
  endingDate?: string;
}

export interface CashFlowWorksheetLineDto extends EntityDto<string> {
  cashFlowForecastNo?: string;
  lineNo: number;
  cashFlowDate?: string;
  sourceType: CashFlowSourceType;
  sourceNo?: string;
  documentNo?: string;
  description?: string;
  amount: number;
}

export interface CreateUpdateCashFlowWorksheetLineDto {
  cashFlowForecastNo: string;
  cashFlowDate?: string;
  sourceType: CashFlowSourceType;
  sourceNo?: string;
  documentNo?: string;
  description?: string;
  amount: number;
}

export interface GetCashFlowLineListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  cashFlowForecastNo?: string;
  filter?: string;
}

export interface CashFlowForecastEntryDto extends EntityDto<string> {
  entryNo: number;
  cashFlowForecastNo?: string;
  cashFlowDate?: string;
  sourceType: CashFlowSourceType;
  sourceNo?: string;
  documentNo?: string;
  description?: string;
  amount: number;
}

export interface SuggestCashFlowInput {
  fromDate?: string;
  toDate?: string;
}

export interface AnalysisMatrixInput {
  startDate?: string;
  periodType: AnalysisPeriodType;
  periodCount: number;
}

export interface AnalysisPeriodDto {
  start?: string;
  end?: string;
  name?: string;
}

export interface CashFlowPeriodRowDto {
  sourceType: CashFlowSourceType;
  amounts: number[];
  total: number;
}

export interface CashFlowByPeriodDto {
  periods: AnalysisPeriodDto[];
  rows: CashFlowPeriodRowDto[];
  totals: number[];
  balances: number[];
}

export interface BudgetMatrixInput extends AnalysisMatrixInput {
  budgetName: string;
  accountFilter?: string;
  withActual: boolean;
  incomeStatementOnly: boolean;
}

export interface BudgetMatrixRowDto {
  accountNo?: string;
  accountName?: string;
  accountType: GLAccountType;
  indentation: number;
  budget: number[];
  actual: number[];
  totalBudget: number;
  totalActual: number;
}

export interface BudgetMatrixDto {
  budgetName?: string;
  periods: AnalysisPeriodDto[];
  rows: BudgetMatrixRowDto[];
}

export interface SetBudgetCellInput {
  budgetName: string;
  accountNo: string;
  periodStart?: string;
  periodEnd?: string;
  amount: number;
}

export interface CopyBudgetInput {
  fromBudgetName: string;
  toBudgetName: string;
  dateChange?: string;
  factor: number;
  replace: boolean;
}
