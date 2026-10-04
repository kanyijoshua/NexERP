import { mapEnumToOptions } from '@abp/ng.core';

export enum PayCalculationMethod {
  FlatAmount = 0,
  PercentOfBasic = 1,
  PercentOfGross = 2,
  TaxBands = 3,
}

export const payCalculationMethodOptions = mapEnumToOptions(PayCalculationMethod);
