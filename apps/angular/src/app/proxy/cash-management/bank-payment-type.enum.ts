import { mapEnumToOptions } from '@abp/ng.core';

export enum BankPaymentType {
  None = 0,
  ComputerCheck = 1,
  ManualCheck = 2,
  ElectronicPayment = 3,
  ElectronicPaymentIat = 4,
}

export const bankPaymentTypeOptions = mapEnumToOptions(BankPaymentType);
