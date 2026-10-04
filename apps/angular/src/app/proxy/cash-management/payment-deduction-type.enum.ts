import { mapEnumToOptions } from '@abp/ng.core';

export enum PaymentDeductionType {
  WithholdingTax = 0,
  WithholdingVat = 1,
  Retention = 2,
}

export const paymentDeductionTypeOptions = mapEnumToOptions(PaymentDeductionType);
