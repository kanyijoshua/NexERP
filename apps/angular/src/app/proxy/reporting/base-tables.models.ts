import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ReportSelectionUsage } from './report-selection-usage.enum';

export interface ReportSelectionDto extends FullAuditedEntityDto<string> {
  usage: ReportSelectionUsage;
  sequence?: string;
  reportId: number;
  customReportLayoutCode?: string;
  useForEmailAttachment: boolean;
  useForEmailBody: boolean;
  emailBodyLayoutCode?: string;
  reportLayoutName?: string;
}

export interface CreateUpdateReportSelectionDto {
  usage: ReportSelectionUsage;
  sequence: string;
  reportId: number;
  customReportLayoutCode?: string;
  useForEmailAttachment: boolean;
  useForEmailBody: boolean;
  emailBodyLayoutCode?: string;
  reportLayoutName?: string;
}

export interface GetReportSelectionListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
}

export interface CustomReportSelectionDto extends FullAuditedEntityDto<string> {
  sourceType: number;
  sourceNo?: string;
  usage: ReportSelectionUsage;
  sequence: number;
  reportId: number;
  customReportLayoutCode?: string;
  sendToEmail?: string;
  useForEmailAttachment: boolean;
  useForEmailBody: boolean;
  emailBodyLayoutCode?: string;
  useEmailFromContact: boolean;
}

export interface CreateUpdateCustomReportSelectionDto {
  sourceType: number;
  sourceNo: string;
  usage: ReportSelectionUsage;
  sequence: number;
  reportId: number;
  customReportLayoutCode?: string;
  sendToEmail?: string;
  useForEmailAttachment: boolean;
  useForEmailBody: boolean;
  emailBodyLayoutCode?: string;
  useEmailFromContact: boolean;
}

export interface GetCustomReportSelectionListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  sourceNo?: string;
}
