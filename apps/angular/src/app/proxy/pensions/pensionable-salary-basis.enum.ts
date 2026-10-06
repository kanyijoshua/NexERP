import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionableSalaryBasis {
  CurrentSalary = 0,
  AverageOfLastYears = 1,
  HighestAnnualSalary = 2,
}

export const pensionableSalaryBasisOptions = mapEnumToOptions(PensionableSalaryBasis);
