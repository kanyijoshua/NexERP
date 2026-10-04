import { mapEnumToOptions } from '@abp/ng.core';

export enum FALedgerEntryFAPostingType {
  AcquisitionCost = 0,
  Depreciation = 1,
  WriteDown = 2,
  Appreciation = 3,
  Custom1 = 4,
  Custom2 = 5,
  ProceedsOnDisposal = 6,
  SalvageValue = 7,
  GainLoss = 8,
  BookValueOnDisposal = 9,
}

export const faLedgerEntryFAPostingTypeOptions = mapEnumToOptions(FALedgerEntryFAPostingType);
