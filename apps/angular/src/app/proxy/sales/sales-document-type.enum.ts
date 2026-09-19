import { mapEnumToOptions } from '@abp/ng.core';

export enum SalesDocumentType {
  Quote = 0,
  Order = 1,
  Invoice = 2,
  CreditMemo = 3,
  BlanketOrder = 4,
  ReturnOrder = 5,
}

export const salesDocumentTypeOptions = mapEnumToOptions(SalesDocumentType);
