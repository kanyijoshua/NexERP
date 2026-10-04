import { mapEnumToOptions } from '@abp/ng.core';

export enum MemberContributionStatus {
  Active = 0,
  Inactive = 1,
  Suspended = 2,
  Other = 3,
}

export const memberContributionStatusOptions = mapEnumToOptions(MemberContributionStatus);
