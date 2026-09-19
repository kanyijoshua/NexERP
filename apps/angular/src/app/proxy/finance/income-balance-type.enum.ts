import { mapEnumToOptions } from '@abp/ng.core';

export enum IncomeBalanceType {
  BalanceSheet = 0,
  IncomeStatement = 1,
}

export const incomeBalanceTypeOptions = mapEnumToOptions(IncomeBalanceType);
