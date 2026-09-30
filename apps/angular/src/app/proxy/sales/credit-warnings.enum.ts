import { mapEnumToOptions } from '@abp/ng.core';

export enum CreditWarnings {
  BothWarnings = 0,
  CreditLimit = 1,
  OverdueBalance = 2,
  NoWarning = 3,
}

export const creditWarningsOptions = mapEnumToOptions(CreditWarnings);
