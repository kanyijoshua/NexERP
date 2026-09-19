import { mapEnumToOptions } from '@abp/ng.core';

export enum ApprovalStatus {
  Created = 0,
  Open = 1,
  Canceled = 2,
  Rejected = 3,
  Approved = 4,
}

export const approvalStatusOptions = mapEnumToOptions(ApprovalStatus);
