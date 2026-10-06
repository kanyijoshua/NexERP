import { mapEnumToOptions } from '@abp/ng.core';

export enum ExcessContributionAllocation {
  EmployeePriority = 0,
  ContributionRate = 1,
}

export const excessContributionAllocationOptions = mapEnumToOptions(ExcessContributionAllocation);
