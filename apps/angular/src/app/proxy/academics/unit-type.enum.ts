import { mapEnumToOptions } from '@abp/ng.core';

export enum UnitType {
  Core = 0,
  Elective = 1,
  Required = 2,
}

export const unitTypeOptions = mapEnumToOptions(UnitType);
