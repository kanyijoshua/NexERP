import { mapEnumToOptions } from '@abp/ng.core';

export enum AgedLedgerKind {
  Receivables = 0,
  Payables = 1,
}

export const agedLedgerKindOptions = mapEnumToOptions(AgedLedgerKind);
