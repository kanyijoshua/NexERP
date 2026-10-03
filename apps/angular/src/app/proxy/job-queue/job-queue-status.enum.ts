import { mapEnumToOptions } from '@abp/ng.core';

export enum JobQueueStatus {
  Ready = 0,
  InProcess = 1,
  Error = 2,
  OnHold = 3,
  Finished = 4,
}

export const jobQueueStatusOptions = mapEnumToOptions(JobQueueStatus);
