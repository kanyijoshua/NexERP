import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { DocumentLineType } from '../documents/document-line-type.enum';

export enum StandardCodeInsertMode {
  Manual = 0,
  Automatic = 1,
}
export const standardCodeInsertModeOptions = mapEnumToOptions(StandardCodeInsertMode);

interface ErpPagedListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
}

export interface StandardSalesCodeDto extends CodeTableDto {
  currencyCode?: string;
  lineCount: number;
}

export interface CreateUpdateStandardSalesCodeDto extends CreateUpdateCodeTableDto {
  currencyCode?: string;
}

export interface StandardSalesLineDto extends EntityDto<string> {
  standardSalesCodeId?: string;
  lineNo: number;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitAmount: number;
  unitOfMeasureCode?: string;
}

export interface CreateUpdateStandardSalesLineDto {
  standardSalesCodeId?: string;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitAmount: number;
  unitOfMeasureCode?: string;
}

export interface GetStandardSalesLineListInput extends ErpPagedListInput {
  standardSalesCodeId?: string;
}

export interface StandardCustomerSalesCodeDto extends FullAuditedEntityDto<string> {
  customerNo?: string;
  code?: string;
  insertOnQuotes: StandardCodeInsertMode;
  insertOnOrders: StandardCodeInsertMode;
  insertOnInvoices: StandardCodeInsertMode;
}

export interface CreateUpdateStandardCustomerSalesCodeDto {
  customerNo: string;
  code: string;
  insertOnQuotes: StandardCodeInsertMode;
  insertOnOrders: StandardCodeInsertMode;
  insertOnInvoices: StandardCodeInsertMode;
}

export interface GetStandardCustomerSalesCodeListInput extends ErpPagedListInput {
  customerNo?: string;
}

export interface InsertStandardLinesInput {
  code: string;
}
