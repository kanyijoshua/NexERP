import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionerPayItemCalculation {
  FlatAmount = 0,
  PercentOfPension = 1,
}

export const pensionerPayItemCalculationOptions = mapEnumToOptions(PensionerPayItemCalculation);
