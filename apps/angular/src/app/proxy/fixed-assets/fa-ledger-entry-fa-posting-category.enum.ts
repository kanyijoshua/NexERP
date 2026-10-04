import { mapEnumToOptions } from '@abp/ng.core';

export enum FALedgerEntryFAPostingCategory {
  None = 0,
  Disposal = 1,
  BalDisposal = 2,
}

export const faLedgerEntryFAPostingCategoryOptions = mapEnumToOptions(FALedgerEntryFAPostingCategory);
