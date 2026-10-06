import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionerPaymentType {
  Bank = 0,
  MobileMoney = 1,
  Cheque = 2,
  Cash = 3,
}

export const pensionerPaymentTypeOptions = mapEnumToOptions(PensionerPaymentType);
