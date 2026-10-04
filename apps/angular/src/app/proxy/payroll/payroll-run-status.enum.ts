import { mapEnumToOptions } from '@abp/ng.core';

export enum PayrollRunStatus {
  Open = 0,
  Calculated = 1,
  Posted = 2,
}

export const payrollRunStatusOptions = mapEnumToOptions(PayrollRunStatus);
