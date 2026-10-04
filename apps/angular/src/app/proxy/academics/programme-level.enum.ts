import { mapEnumToOptions } from '@abp/ng.core';

export enum ProgrammeLevel {
  None = 0,
  Certificate = 1,
  Diploma = 2,
  AdvancedDiploma = 3,
  Degree = 4,
  Masters = 5,
  Doctorate = 6,
  ShortCourse = 7,
}

export const programmeLevelOptions = mapEnumToOptions(ProgrammeLevel);
