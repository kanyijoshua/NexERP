import { mapEnumToOptions } from '@abp/ng.core';

export enum VatCalculationType {
  NormalVat = 0,
  ReverseChargeVat = 1,
  FullVat = 2,
}

export const vatCalculationTypeOptions = mapEnumToOptions(VatCalculationType);
