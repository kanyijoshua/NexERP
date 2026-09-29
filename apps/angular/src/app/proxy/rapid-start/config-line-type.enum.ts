import { mapEnumToOptions } from '@abp/ng.core';

export enum ConfigLineType {
  Area = 0,
  Group = 1,
  Table = 2,
}

export const configLineTypeOptions = mapEnumToOptions(ConfigLineType);
