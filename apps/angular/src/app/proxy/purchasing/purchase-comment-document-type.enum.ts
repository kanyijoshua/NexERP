import { mapEnumToOptions } from '@abp/ng.core';

export enum PurchaseCommentDocumentType {
  Quote = 0,
  Order = 1,
  Invoice = 2,
  CreditMemo = 3,
  BlanketOrder = 4,
  ReturnOrder = 5,
  Receipt = 6,
  PostedInvoice = 7,
  PostedCreditMemo = 8,
  PostedReturnShipment = 9,
}

export const purchaseCommentDocumentTypeOptions = mapEnumToOptions(PurchaseCommentDocumentType);
