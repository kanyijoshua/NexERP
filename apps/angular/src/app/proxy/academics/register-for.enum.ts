import { mapEnumToOptions } from '@abp/ng.core';

export enum RegisterFor {
  Stage = 0,
  Units = 1,
  Supplementary = 2,
  Retake = 3,
}

export const registerForOptions = mapEnumToOptions(RegisterFor);
