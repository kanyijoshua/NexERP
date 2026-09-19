import { mapEnumToOptions } from '@abp/ng.core';

export enum ItemType {
  Inventory = 0,
  Service = 1,
  NonInventory = 2,
}

export const itemTypeOptions = mapEnumToOptions(ItemType);
