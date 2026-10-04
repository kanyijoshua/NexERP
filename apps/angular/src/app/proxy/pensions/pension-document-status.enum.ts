import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionDocumentStatus {
  Open = 0,
  Released = 1,
  PendingApproval = 2,
  Posted = 3,
  Reversed = 4,
  Rejected = 5,
}

export const pensionDocumentStatusOptions = mapEnumToOptions(PensionDocumentStatus);
