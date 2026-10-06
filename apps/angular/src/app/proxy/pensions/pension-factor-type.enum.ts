import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionFactorType {
  EarlyRetirement = 0,
  LateRetirement = 1,
  Commutation = 2,
}

export const pensionFactorTypeOptions = mapEnumToOptions(PensionFactorType);
