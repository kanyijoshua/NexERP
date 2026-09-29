import { mapEnumToOptions } from '@abp/ng.core';

export enum ErpCornerStyle {
  Rounded = 0,
  Square = 1,
}

export const erpCornerStyleOptions = mapEnumToOptions(ErpCornerStyle);
