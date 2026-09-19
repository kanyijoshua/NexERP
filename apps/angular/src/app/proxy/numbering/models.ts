import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateNoSeriesDto {
  code: string;
  description?: string;
  defaultNos: boolean;
  manualNos: boolean;
  dateOrder: boolean;
  lines: NoSeriesLineInputDto[];
}

export interface GetNoSeriesListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
}

export interface NextNoPreviewDto {
  code?: string;
  nextNo?: string;
  manualNos: boolean;
  defaultNos: boolean;
}

export interface NextNoPreviewInput {
  code: string;
  date?: string;
}

export interface NoSeriesDto extends EntityDto<string> {
  code?: string;
  description?: string;
  defaultNos: boolean;
  manualNos: boolean;
  dateOrder: boolean;
  startingNo?: string;
  endingNo?: string;
  lastNoUsed?: string;
  nextNo?: string;
  warning: boolean;
  lines: NoSeriesLineDto[];
}

export interface NoSeriesLineDto extends EntityDto<string> {
  lineNo: number;
  startingDate?: string;
  startingNo?: string;
  endingNo?: string;
  warningNo?: string;
  incrementByNo: number;
  lastNoUsed?: string;
  lastDateUsed?: string;
  open: boolean;
}

export interface NoSeriesLineInputDto {
  id?: string;
  startingDate?: string;
  startingNo: string;
  endingNo?: string;
  warningNo?: string;
  incrementByNo: number;
}
