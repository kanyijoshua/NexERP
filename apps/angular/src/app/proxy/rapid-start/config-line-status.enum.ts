import { mapEnumToOptions } from '@abp/ng.core';

export enum ConfigLineStatus {
  NotStarted = 0,
  InProgress = 1,
  Completed = 2,
  Ignored = 3,
  Blocked = 4,
}

export const configLineStatusOptions = mapEnumToOptions(ConfigLineStatus);
