import { mapEnumToOptions } from '@abp/ng.core';

export enum PartyLedgerType {
  Customer = 0,
  Vendor = 1,
}

export const partyLedgerTypeOptions = mapEnumToOptions(PartyLedgerType);

export enum BankEntryStatementStatus {
  Open = 0,
  BankAccEntryApplied = 1,
  Closed = 2,
}

export const bankEntryStatementStatusOptions = mapEnumToOptions(BankEntryStatementStatus);

export enum CollectionDocumentStatus {
  Open = 0,
  Issued = 1,
}

export const collectionDocumentStatusOptions = mapEnumToOptions(CollectionDocumentStatus);

export enum CollectionLineType {
  Text = 0,
  CustomerLedgerEntry = 1,
  GLAccount = 2,
}

export const collectionLineTypeOptions = mapEnumToOptions(CollectionLineType);

export enum DeferralCalculationMethod {
  StraightLine = 0,
  EqualPerPeriod = 1,
  DaysPerPeriod = 2,
}

export const deferralCalculationMethodOptions = mapEnumToOptions(DeferralCalculationMethod);

export enum DeferralStartDate {
  PostingDate = 0,
  BeginningOfPeriod = 1,
  EndOfPeriod = 2,
  BeginningOfNextPeriod = 3,
}

export const deferralStartDateOptions = mapEnumToOptions(DeferralStartDate);

export enum DeferralSourceType {
  GLJournal = 0,
  Sales = 1,
  Purchase = 2,
}

export const deferralSourceTypeOptions = mapEnumToOptions(DeferralSourceType);

export enum CashFlowSourceType {
  None = 0,
  Receivables = 1,
  Payables = 2,
  SalesOrders = 3,
  PurchaseOrders = 4,
  Budget = 5,
  Manual = 6,
  LiquidFunds = 7,
}

export const cashFlowSourceTypeOptions = mapEnumToOptions(CashFlowSourceType);

export enum CashFlowManualKind {
  Revenue = 0,
  Expense = 1,
}

export const cashFlowManualKindOptions = mapEnumToOptions(CashFlowManualKind);

export enum AnalysisPeriodType {
  Day = 0,
  Week = 1,
  Month = 2,
  Quarter = 3,
  Year = 4,
}

export const analysisPeriodTypeOptions = mapEnumToOptions(AnalysisPeriodType);
