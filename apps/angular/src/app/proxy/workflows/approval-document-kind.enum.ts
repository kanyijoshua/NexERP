import { mapEnumToOptions } from '@abp/ng.core';

export enum ApprovalDocumentKind {
  SalesDocument = 0,
  PurchaseDocument = 1,
}

export const approvalDocumentKindOptions = mapEnumToOptions(ApprovalDocumentKind);
