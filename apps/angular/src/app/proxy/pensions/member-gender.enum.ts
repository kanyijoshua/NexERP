import { mapEnumToOptions } from '@abp/ng.core';

export enum MemberGender {
  None = 0,
  Female = 1,
  Male = 2,
}

export const memberGenderOptions = mapEnumToOptions(MemberGender);
