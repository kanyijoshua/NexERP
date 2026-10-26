import { mapEnumToOptions } from '@abp/ng.core';

export enum ChangeLogEntryType {
  Insertion = 0,
  Modification = 1,
  Deletion = 2,
}

export const changeLogEntryTypeOptions = mapEnumToOptions(ChangeLogEntryType);

export enum ChangeLogFieldFilterType {
  None = 0,
  AllFields = 1,
  SomeFields = 2,
}

export const changeLogFieldFilterTypeOptions = mapEnumToOptions(ChangeLogFieldFilterType);
