import { mapEnumToOptions } from '@abp/ng.core';

export enum CheckLedgerEntryStatementStatus {
  Open = 0,
  BankAccEntryApplied = 1,
  CheckEntryApplied = 2,
  Closed = 3,
}

export const checkLedgerEntryStatementStatusOptions = mapEnumToOptions(CheckLedgerEntryStatementStatus);
