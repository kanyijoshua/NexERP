import { mapEnumToOptions } from '@abp/ng.core';

export enum GLEntrySourceType {
  None = 0,
  Customer = 1,
  Vendor = 2,
  BankAccount = 3,
  FixedAsset = 4,
  Employee = 5,
}

export const glEntrySourceTypeOptions = mapEnumToOptions(GLEntrySourceType);
