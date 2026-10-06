import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionerPayItemType {
  Earning = 0,
  Deduction = 1,
}

export const pensionerPayItemTypeOptions = mapEnumToOptions(PensionerPayItemType);
