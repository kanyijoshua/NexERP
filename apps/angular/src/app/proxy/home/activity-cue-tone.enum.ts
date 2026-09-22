import { mapEnumToOptions } from '@abp/ng.core';

export enum ActivityCueTone {
  Neutral = 0,
  Attention = 1,
  Overdue = 2,
}

export const activityCueToneOptions = mapEnumToOptions(ActivityCueTone);
