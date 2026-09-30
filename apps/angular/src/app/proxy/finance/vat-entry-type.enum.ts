import { mapEnumToOptions } from '@abp/ng.core';

export enum VatEntryType {
  Purchase = 1,
  Sale = 2,
  Settlement = 3,
}

export const vatEntryTypeOptions = mapEnumToOptions(VatEntryType);
