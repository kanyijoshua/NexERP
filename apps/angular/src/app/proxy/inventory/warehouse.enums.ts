import { mapEnumToOptions } from '@abp/ng.core';

export enum CostingMethod {
  Fifo = 0,
  Average = 1,
  Specific = 2,
}

export const costingMethodOptions = mapEnumToOptions(CostingMethod);

export enum ItemJournalTemplateType {
  Item = 0,
  Transfer = 1,
  PhysicalInventory = 2,
  Revaluation = 3,
}

export const itemJournalTemplateTypeOptions = mapEnumToOptions(ItemJournalTemplateType);

export enum ItemTrackingSourceType {
  SalesLine = 0,
  PurchaseLine = 1,
  ItemJournalLine = 2,
  TransferLine = 3,
}

export const itemTrackingSourceTypeOptions = mapEnumToOptions(ItemTrackingSourceType);

export enum TransferStatus {
  Open = 0,
  Released = 1,
}

export const transferStatusOptions = mapEnumToOptions(TransferStatus);
