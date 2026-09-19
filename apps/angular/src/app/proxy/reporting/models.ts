import type { EntityDto } from '@abp/ng.core';

export interface CreateReportLayoutDto {
  reportName: string;
  layoutName: string;
  layoutType: string;
  description?: string;
}

export interface FinancialReportDto {
  reportTitle?: string;
  fromDate?: string;
  toDate?: string;
  rows: FinancialReportLineDto[];
}

export interface FinancialReportLineDto {
  rowNo?: string;
  description?: string;
  amount: number;
}

export interface FinancialReportPeriodInput {
  fromDate?: string;
  toDate?: string;
}

export interface GetReportLayoutsInput {
  reportName: string;
}

export interface ReportLayoutDto extends EntityDto<string> {
  reportName?: string;
  layoutName?: string;
  layoutType?: string;
  description?: string;
  isDefault: boolean;
}

export interface SetDefaultReportLayoutInput {
  reportName: string;
  layoutId?: string;
}
