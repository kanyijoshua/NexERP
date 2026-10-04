import { mapEnumToOptions } from '@abp/ng.core';

export enum TimetableDay {
  Monday = 1,
  Tuesday = 2,
  Wednesday = 3,
  Thursday = 4,
  Friday = 5,
  Saturday = 6,
  Sunday = 7,
}

export const timetableDayOptions = mapEnumToOptions(TimetableDay);
