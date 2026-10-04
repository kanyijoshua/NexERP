import { mapEnumToOptions } from '@abp/ng.core';

export enum CheckLedgerEntryOriginalEntryStatus {
  None = 0,
  Printed = 1,
  Voided = 2,
  Posted = 3,
  FinanciallyVoided = 4,
}

export const checkLedgerEntryOriginalEntryStatusOptions = mapEnumToOptions(CheckLedgerEntryOriginalEntryStatus);
