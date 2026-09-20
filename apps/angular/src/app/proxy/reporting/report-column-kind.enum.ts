import { mapEnumToOptions } from '@abp/ng.core';

export enum ReportColumnKind {
  Text = 0,
  Number = 1,
  Date = 2,
}

export const reportColumnKindOptions = mapEnumToOptions(ReportColumnKind);
