import { mapEnumToOptions } from '@abp/ng.core';

export enum ReportLayoutType {
  Html = 0,
  Word = 1,
  Excel = 2,
}

export const reportLayoutTypeOptions = mapEnumToOptions(ReportLayoutType);
