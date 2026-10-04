import { mapEnumToOptions } from '@abp/ng.core';

export enum MemberStatus {
  None = 0,
  Active = 1,
  Inactive = 2,
  Deferred = 3,
  Dormant = 4,
  Pending = 5,
  DeathInService = 6,
  DeathInDeferment = 7,
  Shortfall = 8,
  Deceased = 9,
  Suspended = 10,
  Open = 11,
}

export const memberStatusOptions = mapEnumToOptions(MemberStatus);
