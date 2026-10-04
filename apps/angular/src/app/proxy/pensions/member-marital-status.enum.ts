import { mapEnumToOptions } from '@abp/ng.core';

export enum MemberMaritalStatus {
  Single = 0,
  Married = 1,
  Separated = 2,
  Divorced = 3,
  Widow = 4,
  Widower = 5,
  Minor = 6,
}

export const memberMaritalStatusOptions = mapEnumToOptions(MemberMaritalStatus);
