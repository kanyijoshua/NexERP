import { mapEnumToOptions } from '@abp/ng.core';

export enum CheckLedgerEntryCheckType {
  TotalCheck = 0,
  PartialCheck = 1,
}

export const checkLedgerEntryCheckTypeOptions = mapEnumToOptions(CheckLedgerEntryCheckType);
