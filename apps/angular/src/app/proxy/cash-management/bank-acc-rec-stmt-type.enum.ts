import { mapEnumToOptions } from '@abp/ng.core';

export enum BankAccRecStmtType {
  BankReconciliation = 0,
  PaymentApplication = 1,
}

export const bankAccRecStmtTypeOptions = mapEnumToOptions(BankAccRecStmtType);
