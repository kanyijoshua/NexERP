import { mapEnumToOptions } from '@abp/ng.core';

export enum BenefitCalculationStatus {
  Open = 0,
  Approved = 1,
}

export const benefitCalculationStatusOptions = mapEnumToOptions(BenefitCalculationStatus);
