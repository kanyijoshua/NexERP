import { mapEnumToOptions } from '@abp/ng.core';

export enum ExchRateAdjmtAccountType {
  Customer = 0,
  Vendor = 1,
  BankAccount = 2,
}

export const exchRateAdjmtAccountTypeOptions = mapEnumToOptions(ExchRateAdjmtAccountType);
