import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { DocumentLineType } from '../documents/document-line-type.enum';
import type { SalesHeaderDto } from './models';

export interface SalesOrderPostingInput {
  ship: boolean;
  invoice: boolean;
  lines: SalesLineQuantitiesInput[];
}

export interface SalesLineQuantitiesInput {
  lineId: string;
  qtyToShip: number;
  qtyToInvoice: number;
}

export interface SalesPostingResultDto {
  document: SalesHeaderDto;
  invoiceNo?: string;
  shipmentNo?: string;
}

export interface MakeOrderInput {
  postingDate?: string;
}

export interface GetShipmentLinesInput {
  shipmentLineIds: string[];
}

export interface CopyDocumentLinesInput {
  sourceDocumentId: string;
}

export interface OpenShipmentLineDto {
  shipmentLineId: string;
  shipmentNo?: string;
  postingDate?: string;
  orderNo?: string;
  lineNo: number;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  qtyNotInvoiced: number;
  unitPrice: number;
}

export interface PostedSalesShipmentDto extends EntityDto<string> {
  no?: string;
  return: boolean;
  orderNo?: string;
  customerId?: string;
  sellToCustomerNo?: string;
  sellToCustomerName?: string;
  postingDate?: string;
  locationCode?: string;
  externalDocumentNo?: string;
  currencyCode?: string;
  shipToName?: string;
  shipToAddress?: string;
  shipToCity?: string;
  shipToPostCode?: string;
  shipToCountryRegionCode?: string;
  lines: PostedSalesShipmentLineDto[];
}

export interface PostedSalesShipmentLineDto extends EntityDto<string> {
  postedSalesShipmentId?: string;
  lineNo: number;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitOfMeasureCode?: string;
  unitPrice: number;
  lineDiscountPercent: number;
  locationCode?: string;
  orderLineId?: string;
  orderLineNo: number;
  itemLedgerEntryId?: string;
  quantityInvoiced: number;
  correction: boolean;
}

export interface PostedSalesInvoiceDto extends EntityDto<string> {
  no?: string;
  preAssignedNo?: string;
  creditMemo: boolean;
  orderNo?: string;
  customerId?: string;
  sellToCustomerNo?: string;
  sellToCustomerName?: string;
  postingDate?: string;
  dueDate?: string;
  currencyCode?: string;
  externalDocumentNo?: string;
  totalAmount: number;
  totalAmountIncludingVat: number;
  lines: PostedSalesInvoiceLineDto[];
}

export interface PostedSalesInvoiceLineDto extends EntityDto<string> {
  postedSalesHeaderId?: string;
  lineNo: number;
  type?: string;
  no?: string;
  description?: string;
  quantity: number;
  unitOfMeasureCode?: string;
  unitPrice: number;
  lineDiscountPercent: number;
  lineAmount: number;
  vatPercent: number;
  vatAmount: number;
  amountIncludingVat: number;
  orderLineNo?: number;
  shipmentNo?: string;
  shipmentLineNo?: number;
}

export interface GetPostedSalesDocumentListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  return?: boolean;
  customerId?: string;
  orderNo?: string;
}
