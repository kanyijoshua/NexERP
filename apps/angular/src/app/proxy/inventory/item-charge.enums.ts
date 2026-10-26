import { mapEnumToOptions } from '@abp/ng.core';

export enum ItemChargeAllocationMethod {
  Equally = 0,
  ByAmount = 1,
  ByWeight = 2,
  ByVolume = 3,
}

export const itemChargeAllocationMethodOptions = mapEnumToOptions(ItemChargeAllocationMethod);

export enum ItemChargeSourceType {
  PurchaseLine = 0,
  SalesLine = 1,
}

export const itemChargeSourceTypeOptions = mapEnumToOptions(ItemChargeSourceType);
