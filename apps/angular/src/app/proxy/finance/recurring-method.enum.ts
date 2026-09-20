import { mapEnumToOptions } from '@abp/ng.core';

export enum RecurringMethod {
  None = 0,
  Fixed = 1,
  Variable = 2,
  ReversingFixed = 3,
  ReversingVariable = 4,
}

export const recurringMethodOptions = mapEnumToOptions(RecurringMethod);
