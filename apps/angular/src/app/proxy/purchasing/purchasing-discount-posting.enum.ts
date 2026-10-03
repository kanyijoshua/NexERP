import { mapEnumToOptions } from '@abp/ng.core';

export enum PurchasingDiscountPosting {
  AllDiscounts = 0,
  InvoiceDiscounts = 1,
  LineDiscounts = 2,
  NoDiscounts = 3,
}

export const purchasingDiscountPostingOptions = mapEnumToOptions(PurchasingDiscountPosting);
