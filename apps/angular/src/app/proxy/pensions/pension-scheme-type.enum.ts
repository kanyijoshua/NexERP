import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionSchemeType {
  PensionFund = 0,
  ProvidentFund = 1,
  InvestmentPool = 2,
}

export const pensionSchemeTypeOptions = mapEnumToOptions(PensionSchemeType);
