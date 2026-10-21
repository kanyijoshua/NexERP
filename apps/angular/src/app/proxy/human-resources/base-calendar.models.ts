import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CalendarRecurringSystem, CalendarWeekday } from './calendar-enums';

export interface BaseCalendarChangeDto extends FullAuditedEntityDto<string> {
  baseCalendarCode?: string;
  recurringSystem: CalendarRecurringSystem;
  date?: string;
  day: CalendarWeekday;
  description?: string;
  nonworking: boolean;
}

export interface CreateUpdateBaseCalendarChangeDto {
  baseCalendarCode: string;
  recurringSystem: CalendarRecurringSystem;
  date?: string;
  day: CalendarWeekday;
  description?: string;
  nonworking: boolean;
}

export interface GetBaseCalendarChangeListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  dynamicFilter?: string;
  baseCalendarCode?: string;
}

export interface CalendarDateDto {
  date: string;
  weekday: CalendarWeekday;
  nonworking: boolean;
  description?: string;
}

export interface GetCalendarDatesInput {
  calendarCode?: string;
  employeeNo?: string;
  fromDate: string;
  toDate: string;
}

export interface CalendarDatesDto {
  calendarCode?: string;
  workingDays: number;
  nonworkingDays: number;
  dates: CalendarDateDto[];
}

export interface CalculateWorkingPeriodInput {
  calendarCode?: string;
  employeeNo?: string;
  startDate: string;
  workingDays: number;
}

export interface WorkingPeriodDto {
  calendarCode?: string;
  startDate: string;
  endDate: string;
  returnDate: string;
  workingDays: number;
  nonworkingDates: CalendarDateDto[];
}

export interface SetCalendarDateStatusInput {
  calendarCode: string;
  date: string;
  nonworking: boolean;
  description?: string;
}
