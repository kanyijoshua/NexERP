import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionSchemeMode {
  Umbrella = 0,
  Single = 1,
}

export const pensionSchemeModeOptions = mapEnumToOptions(PensionSchemeMode);
