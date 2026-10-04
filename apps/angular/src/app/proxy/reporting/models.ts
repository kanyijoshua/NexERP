import type { EntityDto } from '@abp/ng.core';
import type { AccountScheduleTotalingType } from './account-schedule-totaling-type.enum';
import type { AgedLedgerKind } from './aged-ledger-kind.enum';
import type { AgingMethod } from './aging-method.enum';
import type { ColumnLayoutType } from './column-layout-type.enum';
import type { ReportColumnKind } from './report-column-kind.enum';
import type { ReportKind } from './report-kind.enum';
import type { ReportLayoutType } from './report-layout-type.enum';
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

export interface CreateUpdateReportLayoutDto {
  code?: string;
  reportID?: number;
  reportName: string;
  layoutName: string;
  layoutType: ReportLayoutType;
  description?: string;
  fileExtension?: string;
  templateContent: string;
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
  reportName?: string;
}

export interface PreviewReportLayoutInput {
  templateContent: string;
}

export interface ImportRdlcLayoutInput {
  reportName: string;
  layoutName: string;
  description?: string;
  contentBase64: string;
  setAsDefault: boolean;
}

export interface RdlcColumnMatchDto {
  caption?: string;
  rdlcField?: string;
  reportColumnKey?: string;
}

export interface RdlcImportResultDto {
  layout: ReportLayoutDto;
  matchedColumns: number;
  columns: RdlcColumnMatchDto[];
  notes: string[];
}

export interface ReportNameDto {
  name?: string;
  displayName?: string;
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
  code?: string;
  reportID?: number;
  reportName?: string;
  reportCaption?: string;
  layoutName?: string;
  layoutType: ReportLayoutType;
  description?: string;
  fileExtension?: string;
  builtIn: boolean;
  lastModifiedByUser?: string;
  layoutLastUpdated?: string;
  isDefault: boolean;
}

export interface ReportLayoutDetailDto extends ReportLayoutDto {
  templateContent?: string;
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

export interface StandardReportDto {
  id: number;
  code?: string;
  name?: string;
  area?: string;
  hasPeriod: boolean;
  hasAsOfDate: boolean;
  hasNoFilter: boolean;
  hasBudgetName: boolean;
  hasDepreciationBook: boolean;
  hasScheme: boolean;
}

export interface RunStandardReportInput {
  code: string;
  fromDate?: string;
  toDate?: string;
  noFilter?: string;
  budgetName?: string;
  depreciationBookCode?: string;
  schemeCode?: string;
}

export interface StandardReportExportInput extends RunStandardReportInput {
  format: ExportFormat;
}
