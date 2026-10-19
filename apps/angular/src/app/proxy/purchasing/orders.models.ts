import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { DocumentLineType } from '../documents/document-line-type.enum';
import type { PurchaseHeaderDto } from './models';

export interface PurchaseOrderPostingInput {
  receive: boolean;
  invoice: boolean;
  lines: PurchaseLineQuantitiesInput[];
}

export interface PurchaseLineQuantitiesInput {
  lineId: string;
  qtyToReceive: number;
  qtyToInvoice: number;
}

export interface PurchasePostingResultDto {
  document: PurchaseHeaderDto;
  invoiceNo?: string;
  receiptNo?: string;
}

export interface GetReceiptLinesInput {
  receiptLineIds: string[];
}

export interface OpenReceiptLineDto {
  receiptLineId: string;
  receiptNo?: string;
  postingDate?: string;
  orderNo?: string;
  lineNo: number;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  qtyNotInvoiced: number;
  directUnitCost: number;
}

export interface PostedPurchaseReceiptDto extends EntityDto<string> {
  no?: string;
  return: boolean;
  orderNo?: string;
  vendorId?: string;
  buyFromVendorNo?: string;
  buyFromVendorName?: string;
  postingDate?: string;
  locationCode?: string;
  vendorShipmentNo?: string;
  currencyCode?: string;
  lines: PostedPurchaseReceiptLineDto[];
}

export interface PostedPurchaseReceiptLineDto extends EntityDto<string> {
  postedPurchaseReceiptId?: string;
  lineNo: number;
  type: DocumentLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitOfMeasureCode?: string;
  directUnitCost: number;
  lineDiscountPercent: number;
  locationCode?: string;
  orderLineId?: string;
  orderLineNo: number;
  itemLedgerEntryId?: string;
  quantityInvoiced: number;
  correction: boolean;
}

export interface PostedPurchaseInvoiceDto extends EntityDto<string> {
  no?: string;
  preAssignedNo?: string;
  creditMemo: boolean;
  orderNo?: string;
  vendorId?: string;
  buyFromVendorNo?: string;
  buyFromVendorName?: string;
  postingDate?: string;
  dueDate?: string;
  currencyCode?: string;
  vendorInvoiceNo?: string;
  totalAmount: number;
  totalAmountIncludingVat: number;
  lines: PostedPurchaseInvoiceLineDto[];
}

export interface PostedPurchaseInvoiceLineDto extends EntityDto<string> {
  postedPurchaseHeaderId?: string;
  lineNo: number;
  type?: string;
  no?: string;
  description?: string;
  quantity: number;
  unitOfMeasureCode?: string;
  directUnitCost: number;
  lineDiscountPercent: number;
  lineAmount: number;
  vatPercent: number;
  vatAmount: number;
  amountIncludingVat: number;
  orderLineNo?: number;
  receiptNo?: string;
  receiptLineNo?: number;
}

export interface GetPostedPurchaseDocumentListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  return?: boolean;
  vendorId?: string;
  orderNo?: string;
}
