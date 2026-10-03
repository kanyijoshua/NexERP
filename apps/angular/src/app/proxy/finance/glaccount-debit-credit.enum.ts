import { mapEnumToOptions } from '@abp/ng.core';

export enum GLAccountDebitCredit {
  Both = 0,
  Debit = 1,
  Credit = 2,
}

export const glAccountDebitCreditOptions = mapEnumToOptions(GLAccountDebitCredit);
