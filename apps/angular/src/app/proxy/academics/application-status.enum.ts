import { mapEnumToOptions } from '@abp/ng.core';

export enum ApplicationStatus {
  Open = 0,
  Submitted = 1,
  Approved = 2,
  Rejected = 3,
  Admitted = 4,
}

export const applicationStatusOptions = mapEnumToOptions(ApplicationStatus);
