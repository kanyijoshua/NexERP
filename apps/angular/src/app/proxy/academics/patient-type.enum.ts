import { mapEnumToOptions } from '@abp/ng.core';

export enum PatientType {
  Student = 0,
  Employee = 1,
  Other = 2,
}

export const patientTypeOptions = mapEnumToOptions(PatientType);
