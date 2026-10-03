import { mapEnumToOptions } from '@abp/ng.core';

export enum JobQueueIntervalType {
  Minutes = 0,
  Hours = 1,
  Days = 2,
  Weeks = 3,
}

export const jobQueueIntervalTypeOptions = mapEnumToOptions(JobQueueIntervalType);
