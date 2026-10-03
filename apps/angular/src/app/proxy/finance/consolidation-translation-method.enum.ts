import { mapEnumToOptions } from '@abp/ng.core';

export enum ConsolidationTranslationMethod {
  Average = 0,
  Closing = 1,
  Historical = 2,
  Composite = 3,
}

export const consolidationTranslationMethodOptions = mapEnumToOptions(ConsolidationTranslationMethod);
