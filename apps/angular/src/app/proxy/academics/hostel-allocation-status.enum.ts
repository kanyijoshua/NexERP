import { mapEnumToOptions } from '@abp/ng.core';

export enum HostelAllocationStatus {
  Booking = 0,
  Allocated = 1,
  Cleared = 2,
}

export const hostelAllocationStatusOptions = mapEnumToOptions(HostelAllocationStatus);
