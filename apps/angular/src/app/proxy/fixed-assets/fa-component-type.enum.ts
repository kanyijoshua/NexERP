import { mapEnumToOptions } from '@abp/ng.core';

export enum FAComponentType {
  None = 0,
  MainAsset = 1,
  Component = 2,
}

export const faComponentTypeOptions = mapEnumToOptions(FAComponentType);
