import { mapEnumToOptions } from '@abp/ng.core';

export enum ConfigPackageFileFormat {
  Json = 0,
  Xlsx = 1,
}

export const configPackageFileFormatOptions = mapEnumToOptions(ConfigPackageFileFormat);
