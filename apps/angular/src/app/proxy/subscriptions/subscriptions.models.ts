import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { ErpPagedListInput } from '../cost-accounting/cost-accounting.models';
import type { FullAuditedEntityDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';

export enum SubscriptionStatus {
  Draft = 0,
  Active = 1,
  Paused = 2,
  Cancelled = 3,
  Expired = 4,
}
export const subscriptionStatusOptions = mapEnumToOptions(SubscriptionStatus);

export enum SubscriptionLineType {
  GLAccount = 0,
  Item = 1,
}
export const subscriptionLineTypeOptions = mapEnumToOptions(SubscriptionLineType);

export interface SubscriptionPlanDto extends CodeTableDto {
  billingPeriodMonths: number;
  price: number;
  lineType: SubscriptionLineType;
  lineNo?: string;
  trialDays: number;
}

export interface CreateUpdateSubscriptionPlanDto extends CreateUpdateCodeTableDto {
  billingPeriodMonths: number;
  price: number;
  lineType: SubscriptionLineType;
  lineNo: string;
  trialDays: number;
}

export interface CustomerSubscriptionDto extends FullAuditedEntityDto<string> {
  no?: string;
  customerNo?: string;
  customerName?: string;
  planCode?: string;
  quantity: number;
  unitPrice: number;
  startDate?: string;
  endDate?: string;
  nextBillingDate?: string;
  status: SubscriptionStatus;
  lastInvoiceNo?: string;
  lastInvoiceDate?: string;
}

export interface CreateUpdateCustomerSubscriptionDto {
  no?: string;
  customerNo: string;
  planCode: string;
  quantity: number;
  unitPrice: number;
  startDate?: string;
  endDate?: string;
}

export interface GetCustomerSubscriptionListInput extends ErpPagedListInput {
  filter?: string;
  status?: SubscriptionStatus;
}

export interface InvoiceSubscriptionsInput {
  asOfDate?: string;
  post: boolean;
}

export interface SubscriptionInvoicingResultDto {
  invoiceCount: number;
}
