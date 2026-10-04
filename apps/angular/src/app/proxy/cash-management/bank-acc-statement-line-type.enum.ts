import { mapEnumToOptions } from '@abp/ng.core';

export enum BankAccStatementLineType {
  BankAccountLedgerEntry = 0,
  CheckLedgerEntry = 1,
  Difference = 2,
}

export const bankAccStatementLineTypeOptions = mapEnumToOptions(BankAccStatementLineType);
