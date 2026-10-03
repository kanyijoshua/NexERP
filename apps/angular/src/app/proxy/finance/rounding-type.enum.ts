import { mapEnumToOptions } from '@abp/ng.core';

export enum RoundingType {
  Nearest = 0,
  Up = 1,
  Down = 2,
}

export const roundingTypeOptions = mapEnumToOptions(RoundingType);
