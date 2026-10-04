import { mapEnumToOptions } from '@abp/ng.core';

export enum PayslipLineType {
  Earning = 0,
  Deduction = 1,
  EmployerContribution = 2,
}

export const payslipLineTypeOptions = mapEnumToOptions(PayslipLineType);
