import { mapEnumToOptions } from '@abp/ng.core';

export enum MemberExitStatus {
  Open = 0,
  PendingApproval = 1,
  Approved = 2,
  Canceled = 3,
  Rejected = 4,
  Posted = 5,
}

export const memberExitStatusOptions = mapEnumToOptions(MemberExitStatus);
