import { mapEnumToOptions } from '@abp/ng.core';

export enum EntityFilterOperator {
  Equals = 0,
  NotEquals = 1,
  Contains = 2,
  StartsWith = 3,
  GreaterThan = 4,
  GreaterOrEqual = 5,
  LessThan = 6,
  LessOrEqual = 7,
}

export const entityFilterOperatorOptions = mapEnumToOptions(EntityFilterOperator);
