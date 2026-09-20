import { mapEnumToOptions } from '@abp/ng.core';

export enum ExportFormat {
  Csv = 0,
  Xlsx = 1,
  Json = 2,
  Html = 3,
}

export const exportFormatOptions = mapEnumToOptions(ExportFormat);
