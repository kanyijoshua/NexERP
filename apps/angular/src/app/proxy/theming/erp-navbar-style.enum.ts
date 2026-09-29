import { mapEnumToOptions } from '@abp/ng.core';

export enum ErpNavbarStyle {
  Brand = 0,
  Light = 1,
}

export const erpNavbarStyleOptions = mapEnumToOptions(ErpNavbarStyle);
