import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { DocumentLineType } from '../documents/document-line-type.enum';
import type { StandardCodeInsertMode } from '../sales/standard-codes.models';

interface ErpPagedListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
}

export interface StandardPurchaseCodeDto extends CodeTableDto {
  currencyCode?: string;
  lineCount: number;
}

export interface CreateUpdateStandardPurchaseCodeDto extends CreateUpdateCodeTableDto {
  currencyCode?: string;
}

export interface StandardPurchaseLineDto extends EntityDto<string> {
  standardPurchaseCodeId?: string;
  lineNo: number;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitAmount: number;
  unitOfMeasureCode?: string;
}

export interface CreateUpdateStandardPurchaseLineDto {
  standardPurchaseCodeId?: string;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitAmount: number;
  unitOfMeasureCode?: string;
}

export interface GetStandardPurchaseLineListInput extends ErpPagedListInput {
  standardPurchaseCodeId?: string;
}

export interface StandardVendorPurchaseCodeDto extends FullAuditedEntityDto<string> {
  vendorNo?: string;
  code?: string;
  insertOnQuotes: StandardCodeInsertMode;
  insertOnOrders: StandardCodeInsertMode;
  insertOnInvoices: StandardCodeInsertMode;
}

export interface CreateUpdateStandardVendorPurchaseCodeDto {
  vendorNo: string;
  code: string;
  insertOnQuotes: StandardCodeInsertMode;
  insertOnOrders: StandardCodeInsertMode;
  insertOnInvoices: StandardCodeInsertMode;
}

export interface GetStandardVendorPurchaseCodeListInput extends ErpPagedListInput {
  vendorNo?: string;
}
