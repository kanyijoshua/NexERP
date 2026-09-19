import type { GLEntryDocumentType } from './glentry-document-type.enum';
import type { GLAccountType } from './glaccount-type.enum';
import type { GLAccountCategory } from './glaccount-category.enum';
import type { IncomeBalanceType } from './income-balance-type.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateGenJournalBatchDto {
  journalTemplateName: string;
  name: string;
  description?: string;
}

export interface CreateGenJournalLineDto {
  genJournalBatchId?: string;
  postingDate?: string;
  documentType: GLEntryDocumentType;
  documentNo: string;
  accountType: string;
  accountNo: string;
  description?: string;
  amount: number;
  balAccountType?: string;
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

export interface GenJournalBatchDto extends EntityDto<string> {
  journalTemplateName?: string;
  name?: string;
  description?: string;
}

export interface GenJournalLineDto extends EntityDto<string> {
  genJournalBatchId?: string;
  lineNo: number;
  postingDate?: string;
  documentType: GLEntryDocumentType;
  documentNo?: string;
  accountType?: string;
  accountNo?: string;
  description?: string;
  amount: number;
  balAccountType?: string;
  balAccountNo?: string;
}

export interface GenJournalPostingResultDto {
  postedLineCount: number;
  postedDocumentCount: number;
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
