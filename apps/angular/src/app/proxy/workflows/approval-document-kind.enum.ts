import { mapEnumToOptions } from '@abp/ng.core';

export enum ApprovalDocumentKind {
  SalesDocument = 0,
  PurchaseDocument = 1,
  PaymentVoucher = 2,
  PurchaseRequisition = 3,
  Tender = 4,
  LeaveApplication = 5,
}

export const approvalDocumentKindOptions = mapEnumToOptions(ApprovalDocumentKind);
