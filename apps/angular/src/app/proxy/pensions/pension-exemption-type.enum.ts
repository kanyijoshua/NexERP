import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionExemptionType {
  None = 0,
  TaxExempt = 1,
  NonTaxExempt = 2,
}

export const pensionExemptionTypeOptions = mapEnumToOptions(PensionExemptionType);
