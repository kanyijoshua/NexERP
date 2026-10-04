import { mapEnumToOptions } from '@abp/ng.core';

export enum EmployeeGender {
  None = 0,
  Female = 1,
  Male = 2,
  NonBinary = 3,
  SelfDescribed = 4,
  IDontWishToAnswer = 5,
}

export const employeeGenderOptions = mapEnumToOptions(EmployeeGender);
