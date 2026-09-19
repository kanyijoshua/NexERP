import { mapEnumToOptions } from '@abp/ng.core';

export enum GLEntryDocumentType {
  None = 0,
  Payment = 1,
  Invoice = 2,
  CreditMemo = 3,
  FinanceChargeMemo = 4,
  Reminder = 5,
  Refund = 6,
}

export const glEntryDocumentTypeOptions = mapEnumToOptions(GLEntryDocumentType);
