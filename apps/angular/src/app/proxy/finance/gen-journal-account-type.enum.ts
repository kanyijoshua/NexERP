import { mapEnumToOptions } from '@abp/ng.core';

export enum GenJournalAccountType {
  GLAccount = 0,
  Customer = 1,
  Vendor = 2,
}

export const genJournalAccountTypeOptions = mapEnumToOptions(GenJournalAccountType);
