import { mapEnumToOptions } from '@abp/ng.core';

export enum LaundryStatus {
  Received = 0,
  Invoiced = 1,
  Ready = 2,
  Collected = 3,
}

export const laundryStatusOptions = mapEnumToOptions(LaundryStatus);
