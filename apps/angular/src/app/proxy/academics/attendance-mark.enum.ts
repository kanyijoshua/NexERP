import { mapEnumToOptions } from '@abp/ng.core';

export enum AttendanceMark {
  Present = 0,
  Absent = 1,
  Late = 2,
  Excused = 3,
}

export const attendanceMarkOptions = mapEnumToOptions(AttendanceMark);
