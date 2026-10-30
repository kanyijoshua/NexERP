import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto, PagedResultRequestDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';

export enum WarehouseEntryType {
  NegativeAdjmt = 0,
  PositiveAdjmt = 1,
  Movement = 2,
}
export const warehouseEntryTypeOptions = mapEnumToOptions(WarehouseEntryType);

export enum WarehouseActivityType {
  PutAway = 1,
  Pick = 2,
  Movement = 3,
}
export const warehouseActivityTypeOptions = mapEnumToOptions(WarehouseActivityType);

export enum WarehouseActivityStatus {
  Open = 0,
  Registered = 1,
}
export const warehouseActivityStatusOptions = mapEnumToOptions(WarehouseActivityStatus);

export enum WarehouseSourceType {
  None = 0,
  PurchaseReceipt = 1,
  SalesOrder = 2,
}
export const warehouseSourceTypeOptions = mapEnumToOptions(WarehouseSourceType);

export enum ReservationSourceType {
  SalesLine = 0,
  AssemblyLine = 1,
}
export const reservationSourceTypeOptions = mapEnumToOptions(ReservationSourceType);

export enum PhysInvtOrderStatus {
  Open = 0,
  Finished = 1,
}
export const physInvtOrderStatusOptions = mapEnumToOptions(PhysInvtOrderStatus);

export enum AssemblyOrderStatus {
  Open = 0,
  Released = 1,
  Finished = 2,
}
export const assemblyOrderStatusOptions = mapEnumToOptions(AssemblyOrderStatus);

interface ErpPagedListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
}

export interface BinDto extends FullAuditedEntityDto<string> {
  locationCode?: string;
  code?: string;
  description?: string;
  blocked: boolean;
}

export interface CreateUpdateBinDto {
  locationCode: string;
  code: string;
  description?: string;
  blocked: boolean;
}

export interface GetBinListInput extends ErpPagedListInput {
  locationCode?: string;
  filter?: string;
}

export interface BinContentDto {
  locationCode?: string;
  binCode?: string;
  itemNo?: string;
  variantCode?: string;
  quantity: number;
}

export interface GetBinContentListInput extends PagedResultRequestDto {
  locationCode?: string;
  binCode?: string;
  itemNo?: string;
}

export interface WarehouseEntryDto extends EntityDto<string> {
  entryNo: number;
  registeringDate?: string;
  entryType: WarehouseEntryType;
  locationCode?: string;
  binCode?: string;
  itemNo?: string;
  variantCode?: string;
  quantity: number;
  documentNo?: string;
  description?: string;
}

export interface GetWarehouseEntryListInput extends ErpPagedListInput {
  locationCode?: string;
  binCode?: string;
  itemNo?: string;
}

export interface WarehouseActivityDto extends FullAuditedEntityDto<string> {
  no?: string;
  type: WarehouseActivityType;
  locationCode?: string;
  status: WarehouseActivityStatus;
  sourceType: WarehouseSourceType;
  sourceNo?: string;
  description?: string;
  registeredDate?: string;
}

export interface CreateUpdateWarehouseActivityDto {
  locationCode: string;
  description?: string;
}

export interface GetWarehouseActivityListInput extends ErpPagedListInput {
  type?: WarehouseActivityType;
  status?: WarehouseActivityStatus;
  filter?: string;
}

export interface MakeWarehouseDocumentInput {
  sourceNo: string;
}

export interface RegisterWarehouseActivityInput {
  registeringDate?: string;
}

export interface WarehouseActivityLineDto extends EntityDto<string> {
  warehouseActivityHeaderId?: string;
  lineNo: number;
  itemNo?: string;
  variantCode?: string;
  description?: string;
  fromBinCode?: string;
  toBinCode?: string;
  quantity: number;
  qtyToHandle: number;
  qtyHandled: number;
}

export interface CreateUpdateWarehouseActivityLineDto {
  warehouseActivityHeaderId?: string;
  itemNo?: string;
  variantCode?: string;
  description?: string;
  fromBinCode?: string;
  toBinCode?: string;
  quantity: number;
  qtyToHandle?: number;
}

export interface GetWarehouseActivityLineListInput extends ErpPagedListInput {
  warehouseActivityHeaderId?: string;
}

export interface ReservationEntryDto extends FullAuditedEntityDto<string> {
  itemNo?: string;
  locationCode?: string;
  variantCode?: string;
  quantity: number;
  sourceType: ReservationSourceType;
  sourceId?: string;
  sourceDocumentNo?: string;
  sourceLineNo: number;
  shipmentDate?: string;
}

export interface GetReservationEntryListInput extends ErpPagedListInput {
  itemNo?: string;
  sourceDocumentNo?: string;
  sourceId?: string;
}

export interface ReserveDocumentInput {
  documentNo: string;
}

export interface ReservationResultDto {
  lineCount: number;
}

export interface ItemAvailabilityDto {
  itemNo?: string;
  locationCode?: string;
  variantCode?: string;
  inventory: number;
  reserved: number;
  available: number;
}

export interface GetItemAvailabilityInput {
  itemNo: string;
  locationCode?: string;
  variantCode?: string;
}

export interface PhysInvtCountingPeriodDto extends CodeTableDto {
  countFrequencyPerYear: number;
}

export interface CreateUpdatePhysInvtCountingPeriodDto extends CreateUpdateCodeTableDto {
  countFrequencyPerYear: number;
}

export interface PhysInvtOrderDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  locationCode?: string;
  orderDate?: string;
  postingDate?: string;
  status: PhysInvtOrderStatus;
  lineCount: number;
  recordedLineCount: number;
}

export interface CreateUpdatePhysInvtOrderDto {
  no?: string;
  description?: string;
  locationCode: string;
  orderDate?: string;
  postingDate?: string;
}

export interface GetPhysInvtOrderListInput extends ErpPagedListInput {
  status?: PhysInvtOrderStatus;
  filter?: string;
}

export interface CalculatePhysInvtLinesInput {
  onlyItemsDueForCounting: boolean;
  itemCategoryCode?: string;
  includeItemsNotOnInventory: boolean;
}

export interface CalculatePhysInvtLinesResultDto {
  linesAdded: number;
}

export interface PhysInvtOrderLineDto extends EntityDto<string> {
  physInvtOrderHeaderId?: string;
  lineNo: number;
  itemNo?: string;
  description?: string;
  variantCode?: string;
  locationCode?: string;
  binCode?: string;
  lotNo?: string;
  serialNo?: string;
  qtyExpected: number;
  qtyRecorded: number;
  recorded: boolean;
  qtyDifference: number;
}

export interface CreateUpdatePhysInvtOrderLineDto {
  physInvtOrderHeaderId?: string;
  itemNo?: string;
  variantCode?: string;
  binCode?: string;
  lotNo?: string;
  serialNo?: string;
  qtyRecorded: number;
  recorded: boolean;
}

export interface GetPhysInvtOrderLineListInput extends ErpPagedListInput {
  physInvtOrderHeaderId?: string;
}

export interface AssemblyBomComponentDto extends FullAuditedEntityDto<string> {
  parentItemNo?: string;
  lineNo: number;
  componentItemNo?: string;
  description?: string;
  quantityPer: number;
}

export interface CreateUpdateAssemblyBomComponentDto {
  parentItemNo: string;
  componentItemNo: string;
  description?: string;
  quantityPer: number;
}

export interface GetAssemblyBomComponentListInput extends ErpPagedListInput {
  parentItemNo?: string;
  filter?: string;
}

export interface AssemblyOrderDto extends FullAuditedEntityDto<string> {
  no?: string;
  itemNo?: string;
  description?: string;
  variantCode?: string;
  locationCode?: string;
  binCode?: string;
  quantity: number;
  quantityToAssemble: number;
  assembledQuantity: number;
  remainingQuantity: number;
  dueDate?: string;
  postingDate?: string;
  status: AssemblyOrderStatus;
  unitCost: number;
}

export interface CreateUpdateAssemblyOrderDto {
  no?: string;
  itemNo: string;
  description?: string;
  variantCode?: string;
  locationCode?: string;
  binCode?: string;
  quantity: number;
  quantityToAssemble?: number;
  dueDate?: string;
  postingDate?: string;
}

export interface GetAssemblyOrderListInput extends ErpPagedListInput {
  status?: AssemblyOrderStatus;
  filter?: string;
}

export interface AssemblyLineDto extends EntityDto<string> {
  assemblyHeaderId?: string;
  lineNo: number;
  itemNo?: string;
  description?: string;
  variantCode?: string;
  locationCode?: string;
  quantityPer: number;
  quantity: number;
  consumedQuantity: number;
  remainingQuantity: number;
}

export interface GetAssemblyLineListInput extends ErpPagedListInput {
  assemblyHeaderId?: string;
}
