import { mapEnumToOptions } from '@abp/ng.core';

export enum ItemLedgerEntryType {
  Purchase = 0,
  Sale = 1,
  PositiveAdjmt = 2,
  NegativeAdjmt = 3,
  Transfer = 4,
  Consumption = 5,
  Output = 6,
  AssemblyConsumption = 7,
  AssemblyOutput = 8,
}

export const itemLedgerEntryTypeOptions = mapEnumToOptions(ItemLedgerEntryType);
