import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { GenJournalAccountType } from '../finance/gen-journal-account-type.enum';

export enum FAJournalPostingType {
  AcquisitionCost = 0,
  Depreciation = 1,
  WriteDown = 2,
  Appreciation = 3,
  Disposal = 4,
}

export interface FAJournalBatchDto extends EntityDto<string> {
  name?: string;
  description?: string;
  depreciationBookCode?: string;
}

export interface CreateUpdateFAJournalBatchDto {
  name: string;
  description?: string;
  depreciationBookCode?: string;
}

export interface FAJournalLineDto extends EntityDto<string> {
  faJournalBatchId?: string;
  lineNo: number;
  postingDate?: string;
  documentNo?: string;
  faNo?: string;
  depreciationBookCode?: string;
  faPostingType: FAJournalPostingType;
  description?: string;
  amount: number;
  balAccountType?: GenJournalAccountType;
  balAccountNo?: string;
  noOfDepreciationDays: number;
}

export interface CreateUpdateFAJournalLineDto {
  faJournalBatchId?: string;
  postingDate?: string;
  documentNo: string;
  faNo: string;
  depreciationBookCode?: string;
  faPostingType: FAJournalPostingType;
  description?: string;
  amount: number;
  balAccountType?: GenJournalAccountType;
  balAccountNo?: string;
}

export interface GetFAJournalLineListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  faJournalBatchId?: string;
}

export interface CalculateDepreciationInput {
  untilDate?: string;
  postingDate?: string;
  documentNo?: string;
  faNo?: string;
}

export interface FAJournalPostingResultDto {
  lineCount: number;
}

export interface FABookValueDto {
  depreciationBookCode?: string;
  acquisitionDate?: string;
  lastDepreciationDate?: string;
  disposalDate?: string;
  acquisitionCost: number;
  depreciation: number;
  writeDown: number;
  appreciation: number;
  bookValue: number;
  proceeds: number;
  gainLoss: number;
}
