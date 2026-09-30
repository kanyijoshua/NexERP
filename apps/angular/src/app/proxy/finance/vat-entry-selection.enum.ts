import { mapEnumToOptions } from '@abp/ng.core';

export enum VatEntrySelection {
  Open = 0,
  Closed = 1,
  OpenAndClosed = 2,
}

export const vatEntrySelectionOptions = mapEnumToOptions(VatEntrySelection);
