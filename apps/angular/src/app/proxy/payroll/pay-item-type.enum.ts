import { mapEnumToOptions } from '@abp/ng.core';

export enum PayItemType {
  Earning = 0,
  Deduction = 1,
}

export const payItemTypeOptions = mapEnumToOptions(PayItemType);
