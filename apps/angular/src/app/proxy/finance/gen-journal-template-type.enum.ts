import { mapEnumToOptions } from '@abp/ng.core';

export enum GenJournalTemplateType {
  General = 0,
  Sales = 1,
  Purchases = 2,
  CashReceipts = 3,
  Payments = 4,
}

export const genJournalTemplateTypeOptions = mapEnumToOptions(GenJournalTemplateType);
