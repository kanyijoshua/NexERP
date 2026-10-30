import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';

export enum ICSourceType {
  SalesInvoice = 0,
  SalesCreditMemo = 1,
  PurchaseInvoice = 2,
  PurchaseCreditMemo = 3,
}
export const icSourceTypeOptions = mapEnumToOptions(ICSourceType);

export enum ICDocumentType {
  PurchaseInvoice = 0,
  PurchaseCreditMemo = 1,
  SalesInvoice = 2,
  SalesCreditMemo = 3,
}
export const icDocumentTypeOptions = mapEnumToOptions(ICDocumentType);

export enum ICOutboxStatus {
  New = 0,
  Sent = 1,
  Cancelled = 2,
}
export const icOutboxStatusOptions = mapEnumToOptions(ICOutboxStatus);

export enum ICInboxStatus {
  Pending = 0,
  Accepted = 1,
  Rejected = 2,
}
export const icInboxStatusOptions = mapEnumToOptions(ICInboxStatus);

export enum ICLineType {
  Item = 0,
  GLAccount = 1,
}
export const icLineTypeOptions = mapEnumToOptions(ICLineType);

interface ErpPagedListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
}

export interface ICPartnerDto extends CodeTableDto {
  partnerCompanyId?: string;
  customerNo?: string;
  vendorNo?: string;
  defaultGLAccountNo?: string;
  blocked: boolean;
}

export interface CreateUpdateICPartnerDto extends CreateUpdateCodeTableDto {
  partnerCompanyId?: string;
  customerNo?: string;
  vendorNo?: string;
  defaultGLAccountNo?: string;
  blocked: boolean;
}

export interface ICOutboxTransactionDto extends FullAuditedEntityDto<string> {
  transactionNo: number;
  partnerCode?: string;
  sourceType: ICSourceType;
  documentNo?: string;
  postingDate?: string;
  amount: number;
  status: ICOutboxStatus;
  sentDate?: string;
}

export interface GetICTransactionListInput extends ErpPagedListInput {
  partnerCode?: string;
  status?: number;
}

export interface ICSendResultDto {
  sentCount: number;
}

export interface ICInboxTransactionDto extends FullAuditedEntityDto<string> {
  transactionNo: number;
  partnerCode?: string;
  sourceType: ICSourceType;
  documentType: ICDocumentType;
  partnerDocumentNo?: string;
  postingDate?: string;
  dueDate?: string;
  currencyCode?: string;
  amount: number;
  status: ICInboxStatus;
  createdDocumentNo?: string;
}

export interface ICInboxLineDto extends EntityDto<string> {
  icInboxTransactionId?: string;
  lineNo: number;
  type: ICLineType;
  no?: string;
  description?: string;
  quantity: number;
  unitAmount: number;
  lineDiscountPct: number;
  unitOfMeasureCode?: string;
}

export interface GetICInboxLineListInput extends ErpPagedListInput {
  icInboxTransactionId?: string;
}

export interface AcceptICInboxInput {
  postingDate?: string;
}

export interface ICAcceptResultDto {
  documentNo?: string;
}
