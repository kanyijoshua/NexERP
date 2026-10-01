import { mapEnumToOptions } from '@abp/ng.core';

export enum ProfileSource {
  Default = 0,
  Role = 1,
  User = 2,
}

export const profileSourceOptions = mapEnumToOptions(ProfileSource);
