import { mapEnumToOptions } from '@abp/ng.core';

export enum EmployeeStatus {
  Active = 0,
  Inactive = 1,
  Terminated = 2,
}

export const employeeStatusOptions = mapEnumToOptions(EmployeeStatus);
