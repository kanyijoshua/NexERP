import type { EntityDto } from '@abp/ng.core';
import type { AccountScheduleTotalingType } from './account-schedule-totaling-type.enum';
import type { AgedLedgerKind } from './aged-ledger-kind.enum';
import type { AgingMethod } from './aging-method.enum';
import type { ColumnLayoutType } from './column-layout-type.enum';
import type { ReportColumnKind } from './report-column-kind.enum';
import type { ReportKind } from './report-kind.enum';
import type { ExportFormat } from '../exporting/export-format.enum';

export interface AccountScheduleDto extends EntityDto<string> {
  name?: string;
  description?: string;
  defaultColumnLayoutName?: string;
  lineCount: number;
}

export interface AccountScheduleLineDto extends EntityDto<string> {
  accountScheduleId?: string;
  lineNo: number;
  rowNo?: string;
  description?: string;
  totalingType: AccountScheduleTotalingType;
  totaling?: string;
  showOppositeSign: boolean;
  bold: boolean;
  italic: boolean;
  indentation: number;
  hideIfZero: boolean;
}

export interface AgedAccountsInput {
  kind: AgedLedgerKind;
  asOfDate?: string;
  agingMethod: AgingMethod;
  periodLengthDays: number;
  excludeZeroBalances: boolean;
}

export interface ColumnLayoutDto extends EntityDto<string> {
  name?: string;
  description?: string;
  lineCount: number;
}

export interface ColumnLayoutLineDto extends EntityDto<string> {
  columnLayoutId?: string;
  lineNo: number;
  columnNo?: string;
  columnHeader?: string;
  columnType: ColumnLayoutType;
  comparisonDateFormula?: string;
  showOppositeSign: boolean;
}

export interface CreateReportLayoutDto {
  reportName: string;
  layoutName: string;
  layoutType: string;
  description?: string;
}

export interface CreateUpdateAccountScheduleDto {
  name: string;
  description?: string;
  defaultColumnLayoutName?: string;
}

export interface CreateUpdateAccountScheduleLineDto {
  accountScheduleId?: string;
  rowNo: string;
  description?: string;
  totalingType: AccountScheduleTotalingType;
  totaling?: string;
  showOppositeSign: boolean;
  bold: boolean;
  italic: boolean;
  indentation: number;
  hideIfZero: boolean;
}

export interface CreateUpdateColumnLayoutDto {
  name: string;
  description?: string;
}

export interface CreateUpdateColumnLayoutLineDto {
  columnLayoutId?: string;
  columnNo: string;
  columnHeader?: string;
  columnType: ColumnLayoutType;
  comparisonDateFormula?: string;
  showOppositeSign: boolean;
}

export interface FinancialReportPeriodInput {
  fromDate?: string;
  toDate?: string;
  accountFilter?: string;
  excludeZeroBalances: boolean;
}

export interface GetReportLayoutsInput {
  reportName: string;
}

export interface ReportColumnDto {
  key?: string;
  header?: string;
  kind: ReportColumnKind;
}

export interface ReportExportInput {
  report: ReportKind;
  format: ExportFormat;
  fromDate?: string;
  toDate?: string;
  accountFilter?: string;
  excludeZeroBalances: boolean;
  agingMethod: AgingMethod;
  periodLengthDays: number;
  scheduleName?: string;
  columnLayoutName?: string;
}

export interface ReportLayoutDto extends EntityDto<string> {
  reportName?: string;
  layoutName?: string;
  layoutType?: string;
  description?: string;
  isDefault: boolean;
}

export interface ReportResultDto {
  title?: string;
  fromDate?: string;
  toDate?: string;
  columns: ReportColumnDto[];
  rows: ReportRowDto[];
}

export interface ReportRowDto {
  values: Record<string, object>;
  bold: boolean;
  italic: boolean;
  indentation: number;
  drillDownFilter?: string;
}

export interface RunAccountScheduleInput {
  scheduleName: string;
  columnLayoutName?: string;
  fromDate?: string;
  toDate?: string;
}

export interface SetDefaultReportLayoutInput {
  reportName: string;
  layoutId?: string;
}
