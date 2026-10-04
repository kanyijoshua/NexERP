import { mapEnumToOptions } from '@abp/ng.core';

export enum CurrencyInvoiceRoundingType {
  Nearest = 0,
  Up = 1,
  Down = 2,
}

export const currencyInvoiceRoundingTypeOptions = mapEnumToOptions(CurrencyInvoiceRoundingType);
