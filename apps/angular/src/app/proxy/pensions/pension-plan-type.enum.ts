import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionPlanType {
  DefinedBenefit = 0,
  DefinedContribution = 1,
  IndividualPensionPlan = 2,
  Hybrid = 3,
}

export const pensionPlanTypeOptions = mapEnumToOptions(PensionPlanType);
