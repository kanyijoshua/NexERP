import { mapEnumToOptions } from '@abp/ng.core';

export enum InvoicePostingPolicy {
  Allowed = 0,
  Prohibited = 1,
  Mandatory = 2,
}

export const invoicePostingPolicyOptions = mapEnumToOptions(InvoicePostingPolicy);
