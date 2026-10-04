import { mapEnumToOptions } from '@abp/ng.core';

export enum StudentChangeType {
  Deferment = 0,
  Readmission = 1,
  Suspension = 2,
  Discontinuation = 3,
  Graduation = 4,
}

export const studentChangeTypeOptions = mapEnumToOptions(StudentChangeType);
