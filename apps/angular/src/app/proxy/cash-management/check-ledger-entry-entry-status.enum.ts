import { mapEnumToOptions } from '@abp/ng.core';

export enum CheckLedgerEntryEntryStatus {
  None = 0,
  Printed = 1,
  Voided = 2,
  Posted = 3,
  FinanciallyVoided = 4,
  TestPrint = 5,
  Exported = 6,
  Transmitted = 7,
}

export const checkLedgerEntryEntryStatusOptions = mapEnumToOptions(CheckLedgerEntryEntryStatus);
