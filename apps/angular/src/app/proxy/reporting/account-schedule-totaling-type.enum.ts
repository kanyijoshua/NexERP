import { mapEnumToOptions } from '@abp/ng.core';

export enum AccountScheduleTotalingType {
  PostingAccounts = 0,
  TotalAccounts = 1,
  Formula = 2,
  Description = 3,
}

export const accountScheduleTotalingTypeOptions = mapEnumToOptions(AccountScheduleTotalingType);
