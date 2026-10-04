import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionContributionType {
  None = 0,
  EmployeeContribution = 1,
  EmployerContribution = 2,
  EmployerAdditional = 3,
  EmployeeAdditional = 4,
  Pre90Employee = 5,
  Pre90Employer = 6,
}

export const pensionContributionTypeOptions = mapEnumToOptions(PensionContributionType);
