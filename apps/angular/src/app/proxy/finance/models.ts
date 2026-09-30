import type { GenJournalAccountType } from './gen-journal-account-type.enum';
import type { GLAccountType } from './glaccount-type.enum';
import type { GLAccountCategory } from './glaccount-category.enum';
import type { IncomeBalanceType } from './income-balance-type.enum';
import type { GLEntryDocumentType } from './glentry-document-type.enum';
import type { RecurringMethod } from './recurring-method.enum';
import type { GenJournalTemplateType } from './gen-journal-template-type.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { VatCalculationType } from './vat-calculation-type.enum';
import type { VatEntryType } from './vat-entry-type.enum';
import type { GeneralPostingType } from './general-posting-type.enum';

export interface ApplyStandardJournalInput {
  standardJournalId?: string;
  batchId?: string;
  postingDate?: string;
  documentNo: string;
}

export interface CreateGenJournalBatchDto {
  journalTemplateName: string;
  name: string;
  description?: string;
  reasonCode?: string;
  noSeriesCode?: string;
  balAccountType?: GenJournalAccountType;
  balAccountNo?: string;
}

export interface CreateUpdateGLAccountDto {
  vatProdPostingGroup?: string;
  genPostingType?: GeneralPostingType;
  vatBusPostingGroup?: string;
  no?: string;
  name?: string;
  accountType: GLAccountType;
  accountCategory: GLAccountCategory;
  subcategory?: string;
  incomeBalance: IncomeBalanceType;
  directPosting: boolean;
}

export interface CreateUpdateGenJournalLineDto {
  genJournalBatchId?: string;
  postingDate?: string;
  documentDate?: string;
  documentType: GLEntryDocumentType;
  documentNo: string;
  externalDocumentNo?: string;
  accountType: GenJournalAccountType;
  accountNo: string;
  description?: string;
  amount: number;
  balAccountType?: GenJournalAccountType;
  balAccountNo?: string;
  recurringMethod: RecurringMethod;
  recurringFrequency?: string;
  expirationDate?: string;
  appliesToDocNo?: string;
  comment?: string;
  currencyCode?: string;
  genPostingType?: GeneralPostingType;
  vatBusPostingGroup?: string;
  vatProdPostingGroup?: string;
  balGenPostingType?: GeneralPostingType;
  balVatBusPostingGroup?: string;
  balVatProdPostingGroup?: string;
}

export interface CreateUpdateGenJournalTemplateDto {
  name: string;
  description?: string;
  type: GenJournalTemplateType;
  recurring: boolean;
  sourceCode?: string;
  noSeriesCode?: string;
}

export interface GLAccountDto extends FullAuditedEntityDto<string> {
  vatProdPostingGroup?: string;
  genPostingType: GeneralPostingType;
  vatBusPostingGroup?: string;
  no?: string;
  name?: string;
  accountType: GLAccountType;
  accountCategory: GLAccountCategory;
  subcategory?: string;
  incomeBalance: IncomeBalanceType;
  directPosting: boolean;
  blocked: boolean;
  netChange: number;
  balance: number;
}

export interface GLEntryDto extends EntityDto<string> {
  entryNo: number;
  glAccountId?: string;
  glAccountNo?: string;
  postingDate?: string;
  documentType: GLEntryDocumentType;
  documentNo?: string;
  description?: string;
  amount: number;
  sourceNo?: string;
  genBusPostingGroup?: string;
}

export interface GLRegisterDto extends EntityDto<string> {
  no: number;
  transactionNo: number;
  postingDate?: string;
  creationTime?: string;
  userName?: string;
  sourceCode?: string;
  journalBatchName?: string;
  fromEntryNo: number;
  toEntryNo: number;
  fromEmployeeEntryNo: number;
  toEmployeeEntryNo: number;
  reversed: boolean;
  reversedByRegisterNo: number;
  reversedRegisterNo: number;
  isReversible: boolean;
}

export interface GenJournalBatchDto extends EntityDto<string> {
  journalTemplateName?: string;
  name?: string;
  description?: string;
  reasonCode?: string;
  noSeriesCode?: string;
  balAccountType?: GenJournalAccountType;
  balAccountNo?: string;
  recurring: boolean;
  lineCount: number;
  balance: number;
}

export interface GenJournalLineDto extends EntityDto<string> {
  genJournalBatchId?: string;
  lineNo: number;
  postingDate?: string;
  documentDate?: string;
  documentType: GLEntryDocumentType;
  documentNo?: string;
  externalDocumentNo?: string;
  accountType: GenJournalAccountType;
  accountNo?: string;
  description?: string;
  amount: number;
  balAccountType?: GenJournalAccountType;
  balAccountNo?: string;
  recurringMethod: RecurringMethod;
  recurringFrequency?: string;
  expirationDate?: string;
  appliesToDocNo?: string;
  comment?: string;
  currencyCode?: string;
  currencyFactor: number;
  amountLcy: number;
  genPostingType: GeneralPostingType;
  vatBusPostingGroup?: string;
  vatProdPostingGroup?: string;
  vatAmount: number;
  vatBaseAmount: number;
  balGenPostingType: GeneralPostingType;
  balVatBusPostingGroup?: string;
  balVatProdPostingGroup?: string;
  balVatAmount: number;
  balVatBaseAmount: number;
}

export interface GenJournalPostingResultDto {
  registerNo: number;
  transactionNo: number;
  postedLineCount: number;
  postedDocumentCount: number;
  recurringLineCount: number;
  reversingLineCount: number;
}

export interface GenJournalTemplateDto extends EntityDto<string> {
  name?: string;
  description?: string;
  type: GenJournalTemplateType;
  recurring: boolean;
  sourceCode?: string;
  noSeriesCode?: string;
  batchCount: number;
}

export interface GetGLAccountListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  accountCategory?: GLAccountCategory;
  accountType?: GLAccountType;
}

export interface GetGLEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  glAccountId?: string;
  documentNo?: string;
  fromDate?: string;
  toDate?: string;
}

export interface GetGLRegistersInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  dynamicFilter?: string;
  fromDate?: string;
  toDate?: string;
  onlyReversible: boolean;
}

export interface GetGenJournalBatchesInput {
  journalTemplateName?: string;
}

export interface GetStandardJournalsInput {
  journalTemplateName?: string;
}

export interface JournalCheckResultDto {
  isValid: boolean;
  messages: string[];
}

export interface PostingPreviewDto {
  lines: PostingPreviewLineDto[];
  glBalance: number;
}

export interface PostingPreviewLineDto {
  ledger?: string;
  postingDate?: string;
  documentNo?: string;
  accountNo?: string;
  description?: string;
  debitAmount: number;
  creditAmount: number;
}

export interface ReversalResultDto {
  reversedRegisterNo: number;
  reversalRegisterNo: number;
  glEntryCount: number;
  customerEntryCount: number;
  vendorEntryCount: number;
  bankEntryCount: number;
  vatEntryCount: number;
  employeeEntryCount: number;
}

export interface ReverseRegisterInput {
  registerNo: number;
  description?: string;
}

export interface SaveStandardJournalInput {
  batchId?: string;
  code: string;
  description?: string;
}

export interface StandardJournalDto extends EntityDto<string> {
  journalTemplateName?: string;
  code?: string;
  description?: string;
  lineCount: number;
}

export interface UpdateGenJournalBatchDto {
  description?: string;
  reasonCode?: string;
  noSeriesCode?: string;
  balAccountType?: GenJournalAccountType;
  balAccountNo?: string;
}

export interface PostingGroupDto extends CodeTableDto {
}

export interface CreateUpdatePostingGroupDto extends CreateUpdateCodeTableDto {
}


export interface CustomerPostingGroupDto extends PostingGroupDto {
  receivablesAccountNo?: string;
}

export interface CreateUpdateCustomerPostingGroupDto extends CreateUpdatePostingGroupDto {
  receivablesAccountNo: string;
}

export interface VendorPostingGroupDto extends PostingGroupDto {
  payablesAccountNo?: string;
}

export interface CreateUpdateVendorPostingGroupDto extends CreateUpdatePostingGroupDto {
  payablesAccountNo: string;
}

export interface GeneralPostingSetupDto extends FullAuditedEntityDto<string> {
  genBusPostingGroup?: string;
  genProdPostingGroup?: string;
  salesAccountNo?: string;
  salesCreditMemoAccountNo?: string;
  salesDiscountAccountNo?: string;
  purchAccountNo?: string;
  purchCreditMemoAccountNo?: string;
  purchDiscountAccountNo?: string;
  cogsAccountNo?: string;
  inventoryAdjmtAccountNo?: string;
}

export interface CreateUpdateGeneralPostingSetupDto {
  genBusPostingGroup?: string;
  genProdPostingGroup: string;
  salesAccountNo?: string;
  salesCreditMemoAccountNo?: string;
  salesDiscountAccountNo?: string;
  purchAccountNo?: string;
  purchCreditMemoAccountNo?: string;
  purchDiscountAccountNo?: string;
  cogsAccountNo?: string;
  inventoryAdjmtAccountNo?: string;
}

export interface InventoryPostingSetupDto extends FullAuditedEntityDto<string> {
  locationCode?: string;
  inventoryPostingGroup?: string;
  inventoryAccountNo?: string;
}

export interface CreateUpdateInventoryPostingSetupDto {
  locationCode?: string;
  inventoryPostingGroup: string;
  inventoryAccountNo?: string;
}

export interface GeneralLedgerSetupDto {
  allowPostingFrom?: string;
  allowPostingTo?: string;
  lcyCode?: string;
  amountRoundingPrecision: number;
  unitAmountRoundingPrecision: number;
  invRoundingPrecisionLcy: number;
  globalDimension1Code?: string;
  globalDimension2Code?: string;
  bankAccountNos?: string;
}

export interface VatPostingSetupDto extends FullAuditedEntityDto<string> {
  vatBusPostingGroup?: string;
  vatProdPostingGroup?: string;
  description?: string;
  vatIdentifier?: string;
  vatPercent: number;
  vatCalculationType: VatCalculationType;
  salesVatAccountNo?: string;
  purchaseVatAccountNo?: string;
  reverseChrgVatAccountNo?: string;
  blocked: boolean;
}

export interface CreateUpdateVatPostingSetupDto {
  vatBusPostingGroup?: string;
  vatProdPostingGroup?: string;
  description?: string;
  vatIdentifier?: string;
  vatPercent: number;
  vatCalculationType: VatCalculationType;
  salesVatAccountNo?: string;
  purchaseVatAccountNo?: string;
  reverseChrgVatAccountNo?: string;
  blocked: boolean;
}

export interface VatEntryDto extends EntityDto<string> {
  entryNo: number;
  postingDate?: string;
  documentDate?: string;
  documentType: GLEntryDocumentType;
  documentNo?: string;
  type: VatEntryType;
  base: number;
  amount: number;
  vatCalculationType: VatCalculationType;
  billToPayToNo?: string;
  vatBusPostingGroup?: string;
  vatProdPostingGroup?: string;
  vatIdentifier?: string;
  vatPercent: number;
  transactionNo: number;
  closed: boolean;
  closedByEntryNo: number;
  reversed: boolean;
}

export interface GetVatEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  type?: VatEntryType;
  fromDate?: string;
  toDate?: string;
}

export interface PaymentTermsDto extends CodeTableDto {
  dueDateCalculation?: string;
  discountDateCalculation?: string;
  discountPercent: number;
}

export interface CreateUpdatePaymentTermsDto extends CreateUpdateCodeTableDto {
  dueDateCalculation?: string;
  discountDateCalculation?: string;
  discountPercent: number;
}

export interface CurrencyDto extends CodeTableDto {
  symbol?: string;
  amountRoundingPrecision: number;
  realizedGainsAccountNo?: string;
  realizedLossesAccountNo?: string;
  unrealizedGainsAccountNo?: string;
  unrealizedLossesAccountNo?: string;
}

export interface CreateUpdateCurrencyDto extends CreateUpdateCodeTableDto {
  symbol?: string;
  amountRoundingPrecision: number;
  realizedGainsAccountNo?: string;
  realizedLossesAccountNo?: string;
  unrealizedGainsAccountNo?: string;
  unrealizedLossesAccountNo?: string;
}

export interface CurrencyExchangeRateDto extends FullAuditedEntityDto<string> {
  currencyCode?: string;
  startingDate?: string;
  exchangeRateAmount: number;
  relationalExchangeRateAmount: number;
}

export interface CreateUpdateCurrencyExchangeRateDto {
  currencyCode: string;
  startingDate?: string;
  exchangeRateAmount: number;
  relationalExchangeRateAmount: number;
}

export interface AccountingPeriodDto extends FullAuditedEntityDto<string> {
  startingDate?: string;
  name?: string;
  newFiscalYear: boolean;
  closed: boolean;
  dateLocked: boolean;
}

export interface GetAccountingPeriodListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
}

export interface NewFiscalYearDto {
  startingDate?: string;
  noOfPeriods: number;
  periodLength?: string;
}

export interface FiscalYearClosedDto {
  fromDate?: string;
  toDate?: string;
}
