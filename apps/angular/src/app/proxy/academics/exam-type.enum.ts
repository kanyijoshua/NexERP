import { mapEnumToOptions } from '@abp/ng.core';

export enum ExamType {
  Assignment = 0,
  Cat = 1,
  Cat2 = 2,
  FinalExam = 3,
}

export const examTypeOptions = mapEnumToOptions(ExamType);
