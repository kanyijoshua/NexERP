import { mapEnumToOptions } from '@abp/ng.core';

export enum EmployeeQualificationType {
  None = 0,
  Internal = 1,
  External = 2,
  PreviousPosition = 3,
}

export const employeeQualificationTypeOptions = mapEnumToOptions(EmployeeQualificationType);
