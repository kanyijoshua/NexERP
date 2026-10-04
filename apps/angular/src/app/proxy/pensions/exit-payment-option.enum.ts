import { mapEnumToOptions } from '@abp/ng.core';

export enum ExitPaymentOption {
  None = 0,
  PayEmployeeAndEmployer = 1,
  PayEmployee = 2,
  PayEmployeeEmployerDeferred = 3,
}

export const exitPaymentOptionOptions = mapEnumToOptions(ExitPaymentOption);
