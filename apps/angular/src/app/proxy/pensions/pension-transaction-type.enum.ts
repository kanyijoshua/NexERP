import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionTransactionType {
  None = 0,
  Contribution = 1,
  Withdrawal = 2,
  Interest = 3,
  Reserve = 4,
  Levies = 5,
  TaxOnInterest = 6,
  ReserveWithdrawal = 7,
  InterestWithdrawal = 8,
  GroupLife = 9,
}

export const pensionTransactionTypeOptions = mapEnumToOptions(PensionTransactionType);
