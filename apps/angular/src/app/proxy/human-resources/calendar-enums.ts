import { mapEnumToOptions } from '@abp/ng.core';

export enum CalendarRecurringSystem {
  None = 0,
  AnnualRecurring = 1,
  WeeklyRecurring = 2,
}

export const calendarRecurringSystemOptions = mapEnumToOptions(CalendarRecurringSystem);

export enum CalendarWeekday {
  None = 0,
  Monday = 1,
  Tuesday = 2,
  Wednesday = 3,
  Thursday = 4,
  Friday = 5,
  Saturday = 6,
  Sunday = 7,
}

export const calendarWeekdayOptions = mapEnumToOptions(CalendarWeekday);
