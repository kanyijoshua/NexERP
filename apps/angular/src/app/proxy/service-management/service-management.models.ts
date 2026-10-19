import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';

export enum ServiceOrderStatus {
  Pending = 0,
  InProcess = 1,
  OnHold = 2,
  Finished = 3,
}
export const serviceOrderStatusOptions = mapEnumToOptions(ServiceOrderStatus);

export enum ServicePriority {
  Low = 0,
  Medium = 1,
  High = 2,
}
export const servicePriorityOptions = mapEnumToOptions(ServicePriority);

export enum ServiceLineType {
  Item = 0,
  Resource = 1,
  Cost = 2,
}
export const serviceLineTypeOptions = mapEnumToOptions(ServiceLineType);

export enum ServiceContractStatus {
  Draft = 0,
  Signed = 1,
  Cancelled = 2,
}
export const serviceContractStatusOptions = mapEnumToOptions(ServiceContractStatus);

export enum ServiceInvoicePeriod {
  Month = 1,
  TwoMonths = 2,
  Quarter = 3,
  HalfYear = 6,
  Year = 12,
}
export const serviceInvoicePeriodOptions = mapEnumToOptions(ServiceInvoicePeriod);

export enum ServiceLedgerEntryType {
  Sale = 0,
  Contract = 1,
}
export const serviceLedgerEntryTypeOptions = mapEnumToOptions(ServiceLedgerEntryType);

export interface PagedServiceInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
}

export interface ServiceMgtSetupDto {
  serviceOrderNos?: string;
  serviceItemNos?: string;
  serviceContractNos?: string;
  serviceRevenueAccountNo?: string;
  contractRevenueAccountNo?: string;
  defaultWarrantyMonths: number;
}

export interface ServiceOrderTypeDto extends CodeTableDto {}
export interface CreateUpdateServiceOrderTypeDto extends CreateUpdateCodeTableDto {}
export interface FaultCodeDto extends CodeTableDto {}
export interface CreateUpdateFaultCodeDto extends CreateUpdateCodeTableDto {}
export interface ResolutionCodeDto extends CodeTableDto {}
export interface CreateUpdateResolutionCodeDto extends CreateUpdateCodeTableDto {}

export interface ServiceItemDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  customerNo?: string;
  customerName?: string;
  itemNo?: string;
  serialNo?: string;
  installationDate?: string;
  warrantyStartingDate?: string;
  warrantyEndingDate?: string;
  blocked: boolean;
}

export interface CreateUpdateServiceItemDto {
  no?: string;
  description?: string;
  customerNo: string;
  itemNo?: string;
  serialNo?: string;
  installationDate?: string;
  warrantyStartingDate?: string;
  warrantyEndingDate?: string;
  blocked: boolean;
}

export interface ServiceContractDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  customerNo?: string;
  customerName?: string;
  status: ServiceContractStatus;
  startingDate?: string;
  expirationDate?: string;
  invoicePeriod: ServiceInvoicePeriod;
  nextInvoiceDate?: string;
  lastInvoiceDate?: string;
  lastInvoiceNo?: string;
  annualAmount: number;
  periodAmount: number;
}

export interface CreateUpdateServiceContractDto {
  no?: string;
  description?: string;
  customerNo: string;
  startingDate?: string;
  expirationDate?: string;
  invoicePeriod: ServiceInvoicePeriod;
}

export interface ServiceContractLineDto extends EntityDto<string> {
  serviceContractId?: string;
  lineNo: number;
  serviceItemNo?: string;
  description?: string;
  lineValue: number;
}

export interface CreateUpdateServiceContractLineDto {
  serviceContractId?: string;
  serviceItemNo?: string;
  description?: string;
  lineValue: number;
}

export interface GetServiceContractLineListInput extends PagedServiceInput {
  serviceContractId?: string;
}

export interface InvoiceServiceContractsInput {
  asOfDate?: string;
}

export interface ServiceContractInvoicingResultDto {
  invoicedPeriods: number;
}

export interface ServiceItemLineDto extends EntityDto<string> {
  serviceOrderId?: string;
  lineNo: number;
  serviceItemNo?: string;
  description?: string;
  itemNo?: string;
  serialNo?: string;
  faultCode?: string;
  resolutionCode?: string;
  faultComment?: string;
  warranty: boolean;
}

export interface CreateUpdateServiceItemLineDto {
  serviceOrderId?: string;
  serviceItemNo: string;
  faultCode?: string;
  resolutionCode?: string;
  faultComment?: string;
  warranty?: boolean;
}

export interface ServiceLineDto extends EntityDto<string> {
  serviceOrderId?: string;
  lineNo: number;
  serviceItemNo?: string;
  type: ServiceLineType;
  no?: string;
  description?: string;
  locationCode?: string;
  quantity: number;
  unitPrice: number;
  unitCost: number;
  lineDiscountPct: number;
  lineAmount: number;
  warranty: boolean;
  quantityPosted: number;
}

export interface CreateUpdateServiceLineDto {
  serviceOrderId?: string;
  serviceItemNo?: string;
  type: ServiceLineType;
  no: string;
  description?: string;
  locationCode?: string;
  quantity: number;
  unitPrice?: number;
  unitCost?: number;
  lineDiscountPct: number;
  warranty?: boolean;
}

export interface GetServiceOrderLineListInput extends PagedServiceInput {
  serviceOrderId?: string;
}

export interface ServiceOrderDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  customerNo?: string;
  customerName?: string;
  serviceOrderType?: string;
  status: ServiceOrderStatus;
  priority: ServicePriority;
  orderDate?: string;
  resourceNo?: string;
  contractNo?: string;
  finishingDate?: string;
  lastInvoiceNo?: string;
  amount: number;
  warrantyAmount: number;
  itemLines: ServiceItemLineDto[];
  lines: ServiceLineDto[];
}

export interface CreateUpdateServiceOrderDto {
  no?: string;
  description?: string;
  customerNo: string;
  serviceOrderType?: string;
  priority: ServicePriority;
  orderDate?: string;
  resourceNo?: string;
  contractNo?: string;
}

export interface GetServiceOrderListInput extends PagedServiceInput {
  status?: ServiceOrderStatus;
  resourceNo?: string;
}

export interface ServicePostingResultDto {
  invoiceNo?: string;
  lineCount: number;
  amount: number;
  warrantyAmount: number;
}

export interface ServiceLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  entryType: ServiceLedgerEntryType;
  postingDate?: string;
  documentNo?: string;
  serviceOrderNo?: string;
  serviceContractNo?: string;
  serviceItemNo?: string;
  customerNo?: string;
  type: ServiceLineType;
  no?: string;
  description?: string;
  quantity: number;
  costAmount: number;
  amount: number;
  warranty: boolean;
}

export interface WarrantyLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  postingDate?: string;
  documentNo?: string;
  serviceOrderNo?: string;
  serviceItemNo?: string;
  itemNo?: string;
  serialNo?: string;
  customerNo?: string;
  type: ServiceLineType;
  no?: string;
  description?: string;
  quantity: number;
  amount: number;
  costAmount: number;
}

export interface GetServiceLedgerListInput extends PagedServiceInput {
  serviceOrderNo?: string;
  serviceItemNo?: string;
}
