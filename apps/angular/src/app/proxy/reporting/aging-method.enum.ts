import { mapEnumToOptions } from '@abp/ng.core';

export enum AgingMethod {
  DueDate = 0,
  PostingDate = 1,
}

export const agingMethodOptions = mapEnumToOptions(AgingMethod);
