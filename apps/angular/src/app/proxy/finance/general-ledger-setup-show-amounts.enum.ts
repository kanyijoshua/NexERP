import { mapEnumToOptions } from '@abp/ng.core';

export enum GeneralLedgerSetupShowAmounts {
  AmountOnly = 0,
  DebitCreditOnly = 1,
  AllAmounts = 2,
}

export const generalLedgerSetupShowAmountsOptions = mapEnumToOptions(GeneralLedgerSetupShowAmounts);
