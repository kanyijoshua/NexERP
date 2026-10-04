import { mapEnumToOptions } from '@abp/ng.core';

export enum ApprovalDocumentType {
  Quote = 0,
  Order = 1,
  Invoice = 2,
  CreditMemo = 3,
  BlanketOrder = 4,
  ReturnOrder = 5,
  None = 6,
  Payment = 7,
}

export const approvalDocumentTypeOptions = mapEnumToOptions(ApprovalDocumentType);
