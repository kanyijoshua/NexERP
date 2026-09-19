import { mapEnumToOptions } from '@abp/ng.core';

export enum GLAccountCategory {
  Assets = 0,
  Liabilities = 1,
  Equity = 2,
  Income = 3,
  CostOfGoodsSold = 4,
  Expense = 5,
}

export const glAccountCategoryOptions = mapEnumToOptions(GLAccountCategory);
