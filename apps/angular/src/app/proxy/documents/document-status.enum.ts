import { mapEnumToOptions } from '@abp/ng.core';

export enum DocumentStatus {
  Open = 0,
  Released = 1,
  PendingApproval = 2,
  PendingPrepayment = 3,
  Posted = 4,
  Cancelled = 5,
}

export const documentStatusOptions = mapEnumToOptions(DocumentStatus);
