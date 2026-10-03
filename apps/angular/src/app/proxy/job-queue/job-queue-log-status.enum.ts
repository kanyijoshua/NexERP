import { mapEnumToOptions } from '@abp/ng.core';

export enum JobQueueLogStatus {
  Success = 0,
  Error = 1,
  InProcess = 2,
}

export const jobQueueLogStatusOptions = mapEnumToOptions(JobQueueLogStatus);
