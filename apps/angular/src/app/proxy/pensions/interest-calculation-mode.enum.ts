import { mapEnumToOptions } from '@abp/ng.core';

export enum InterestCalculationMode {
  CompoundMonthly = 0,
  CompoundDaily = 1,
  Simple = 2,
}

export const interestCalculationModeOptions = mapEnumToOptions(InterestCalculationMode);
