import { mapEnumToOptions } from '@abp/ng.core';

export enum FALedgerEntryDisposalCalculationMethod {
  None = 0,
  Net = 1,
  Gross = 2,
}

export const faLedgerEntryDisposalCalculationMethodOptions = mapEnumToOptions(FALedgerEntryDisposalCalculationMethod);
