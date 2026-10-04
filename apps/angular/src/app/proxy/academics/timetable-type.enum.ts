import { mapEnumToOptions } from '@abp/ng.core';

export enum TimetableType {
  Teaching = 0,
  Exam = 1,
}

export const timetableTypeOptions = mapEnumToOptions(TimetableType);
