import { mapEnumToOptions } from '@abp/ng.core';

export enum EmailDocumentType {
  SalesQuote = 0,
  SalesOrder = 1,
  SalesInvoice = 2,
  SalesCreditMemo = 3,
  PostedSalesInvoice = 4,
  PostedSalesCreditMemo = 5,
  PurchaseQuote = 6,
  PurchaseOrder = 7,
  PurchaseInvoice = 8,
  PostedPurchaseInvoice = 9,
}

export const emailDocumentTypeOptions = mapEnumToOptions(EmailDocumentType);

export enum EmailDeliveryStatus {
  Queued = 0,
  Sent = 1,
  Failed = 2,
}

export const emailDeliveryStatusOptions = mapEnumToOptions(EmailDeliveryStatus);
