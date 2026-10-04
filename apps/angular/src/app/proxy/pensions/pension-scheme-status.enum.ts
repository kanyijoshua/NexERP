import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionSchemeStatus {
  Open = 0,
  Closed = 1,
}

export const pensionSchemeStatusOptions = mapEnumToOptions(PensionSchemeStatus);
