import { mapEnumToOptions } from '@abp/ng.core';

export enum ApplicationMethod {
  Manual = 0,
  ApplyToOldest = 1,
}

export const applicationMethodOptions = mapEnumToOptions(ApplicationMethod);
