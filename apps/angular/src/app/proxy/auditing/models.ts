import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ChangeLogEntryType, ChangeLogFieldFilterType } from './change-log.enums';

export interface ChangeLogSetupDto {
  changeLogActivated: boolean;
}

export interface ChangeLogTableDto extends FullAuditedEntityDto<string> {
  entityName?: string;
  logInsertion: ChangeLogFieldFilterType;
  logModification: ChangeLogFieldFilterType;
  logDeletion: ChangeLogFieldFilterType;
  trackedFields: string[];
}

export interface CreateUpdateChangeLogTableDto {
  entityName: string;
  logInsertion: ChangeLogFieldFilterType;
  logModification: ChangeLogFieldFilterType;
  logDeletion: ChangeLogFieldFilterType;
  trackedFields: string[];
}

export interface GetChangeLogTableListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
}

export interface ChangeLogLoggableTableDto {
  entityName?: string;
  fields: string[];
}

export interface ChangeLogEntryDto {
  id?: string;
  changedAt?: string;
  userId?: string;
  userName?: string;
  entityName?: string;
  recordId?: string;
  primaryKey?: string;
  fieldName?: string;
  typeOfChange: ChangeLogEntryType;
  oldValue?: string;
  newValue?: string;
}

export interface GetChangeLogEntryListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  entityName?: string;
  recordId?: string;
  fieldName?: string;
  typeOfChange?: ChangeLogEntryType;
  fromDate?: string;
  toDate?: string;
}

export interface PurgeChangeLogEntriesInput {
  before: string;
  entityName?: string;
}

export interface PurgeChangeLogEntriesResultDto {
  deleted: number;
}
