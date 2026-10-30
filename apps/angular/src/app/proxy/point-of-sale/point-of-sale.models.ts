import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { ErpPagedListInput } from '../cost-accounting/cost-accounting.models';
import type { EntityDto, FullAuditedEntityDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';

export enum PosSessionStatus {
  Open = 0,
  Closed = 1,
}
export const posSessionStatusOptions = mapEnumToOptions(PosSessionStatus);

export enum PosOrderStatus {
  Open = 0,
  Paid = 1,
  Cancelled = 2,
}
export const posOrderStatusOptions = mapEnumToOptions(PosOrderStatus);

export enum PosPaymentMethod {
  Cash = 0,
  Card = 1,
}
export const posPaymentMethodOptions = mapEnumToOptions(PosPaymentMethod);

export interface PosTerminalDto extends CodeTableDto {
  locationCode?: string;
  walkInCustomerNo?: string;
  cashBankAccountNo?: string;
  cardBankAccountNo?: string;
  blocked: boolean;
}

export interface CreateUpdatePosTerminalDto extends CreateUpdateCodeTableDto {
  locationCode?: string;
  walkInCustomerNo: string;
  cashBankAccountNo: string;
  cardBankAccountNo?: string;
  blocked: boolean;
}

export interface PosSessionDto extends FullAuditedEntityDto<string> {
  no?: string;
  terminalCode?: string;
  openedAt?: string;
  openedBy?: string;
  openingFloat: number;
  status: PosSessionStatus;
  closedAt?: string;
  cashSales: number;
  cardSales: number;
  expectedCash: number;
  countedCash: number;
  difference: number;
}

export interface GetPosSessionListInput extends ErpPagedListInput {
  terminalCode?: string;
  status?: PosSessionStatus;
}

export interface OpenPosSessionInput {
  terminalCode: string;
  openingFloat: number;
}

export interface ClosePosSessionInput {
  countedCash: number;
}

export interface PosOrderDto extends FullAuditedEntityDto<string> {
  no?: string;
  sessionId?: string;
  terminalCode?: string;
  customerNo?: string;
  orderDate?: string;
  status: PosOrderStatus;
  paymentMethod: PosPaymentMethod;
  subtotal: number;
  amountTendered: number;
  change: number;
  totalPaid: number;
  postedInvoiceNo?: string;
}

export interface GetPosOrderListInput extends ErpPagedListInput {
  sessionId?: string;
  status?: PosOrderStatus;
}

export interface StartPosOrderInput {
  sessionId?: string;
  customerNo?: string;
}

export interface RingUpInput {
  itemNo: string;
  quantity: number;
  unitPrice?: number;
  discountPct: number;
}

export interface PayPosOrderInput {
  method: PosPaymentMethod;
  amountTendered: number;
}

export interface PosOrderLineDto extends EntityDto<string> {
  posOrderId?: string;
  lineNo: number;
  itemNo?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  discountPct: number;
  lineAmount: number;
}

export interface GetPosOrderLineListInput extends ErpPagedListInput {
  posOrderId?: string;
}
