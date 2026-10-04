import { mapEnumToOptions } from '@abp/ng.core';

export enum StudyMode {
  None = 0,
  FullTime = 1,
  PartTime = 2,
  Evening = 3,
  Weekend = 4,
  DistanceLearning = 5,
}

export const studyModeOptions = mapEnumToOptions(StudyMode);
