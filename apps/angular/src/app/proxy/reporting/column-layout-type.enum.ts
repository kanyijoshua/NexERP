import { mapEnumToOptions } from '@abp/ng.core';

export enum ColumnLayoutType {
  NetChange = 0,
  BalanceAtDate = 1,
  BeginningBalance = 2,
  YearToDateNetChange = 3,
}

export const columnLayoutTypeOptions = mapEnumToOptions(ColumnLayoutType);
