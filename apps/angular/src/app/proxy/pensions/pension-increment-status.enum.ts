import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionIncrementStatus {
  Open = 0,
  Applied = 1,
}

export const pensionIncrementStatusOptions = mapEnumToOptions(PensionIncrementStatus);
