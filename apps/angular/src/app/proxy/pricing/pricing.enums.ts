import { mapEnumToOptions } from '@abp/ng.core';

export enum PriceType {
  Sale = 0,
  Purchase = 1,
}

export const priceTypeOptions = mapEnumToOptions(PriceType);

export enum PriceSourceType {
  AllCustomers = 0,
  CustomerPriceGroup = 1,
  Customer = 2,
  AllVendors = 10,
  Vendor = 11,
}

export const priceSourceTypeOptions = mapEnumToOptions(PriceSourceType);

export enum PriceAssetType {
  Item = 0,
  ItemCategory = 1,
}

export const priceAssetTypeOptions = mapEnumToOptions(PriceAssetType);

export enum PriceListStatus {
  Draft = 0,
  Active = 1,
  Inactive = 2,
}

export const priceListStatusOptions = mapEnumToOptions(PriceListStatus);
