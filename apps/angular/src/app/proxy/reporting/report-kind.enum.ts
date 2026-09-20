import { mapEnumToOptions } from '@abp/ng.core';

export enum ReportKind {
  TrialBalance = 0,
  IncomeStatement = 1,
  BalanceSheet = 2,
  GeneralLedgerDetail = 3,
  AgedReceivables = 4,
  AgedPayables = 5,
  AccountSchedule = 6,
}

export const reportKindOptions = mapEnumToOptions(ReportKind);
