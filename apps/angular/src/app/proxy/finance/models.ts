import type { GenJournalAccountType } from './gen-journal-account-type.enum';
import type { GLAccountType } from './glaccount-type.enum';
import type { GLAccountCategory } from './glaccount-category.enum';
import type { IncomeBalanceType } from './income-balance-type.enum';
import type { GLEntryDocumentType } from './glentry-document-type.enum';
import type { RecurringMethod } from './recurring-method.enum';
import type { GenJournalTemplateType } from './gen-journal-template-type.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

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
  filter?: string;
  accountCategory?: GLAccountCategory;
  accountType?: GLAccountType;
}

export interface GetGLEntryListInput extends PagedAndSortedResultRequestDto {
  glAccountId?: string;
  documentNo?: string;
  fromDate?: string;
  toDate?: string;
}

export interface GetGLRegistersInput extends PagedAndSortedResultRequestDto {
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
