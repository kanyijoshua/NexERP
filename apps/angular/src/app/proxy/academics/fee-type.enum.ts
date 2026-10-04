import { mapEnumToOptions } from '@abp/ng.core';

export enum FeeType {
  NormalCharge = 0,
  OptionalCharge = 1,
}

export const feeTypeOptions = mapEnumToOptions(FeeType);
