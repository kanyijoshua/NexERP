import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';

export enum ReplenishmentSystem {
  Purchase = 0,
  ProdOrder = 1,
  Transfer = 2,
}
export const replenishmentSystemOptions = mapEnumToOptions(ReplenishmentSystem);

export enum ReorderPolicy {
  None = 0,
  FixedReorderQty = 1,
  MaximumQty = 2,
  LotForLot = 3,
}
export const reorderPolicyOptions = mapEnumToOptions(ReorderPolicy);

export enum ProductionBomStatus {
  New = 0,
  Certified = 1,
  Closed = 2,
}
export const productionBomStatusOptions = mapEnumToOptions(ProductionBomStatus);

export enum ProductionBomLineType {
  Item = 0,
  ProductionBom = 1,
}
export const productionBomLineTypeOptions = mapEnumToOptions(ProductionBomLineType);

export enum RoutingLineType {
  WorkCenter = 0,
  MachineCenter = 1,
}
export const routingLineTypeOptions = mapEnumToOptions(RoutingLineType);

export enum ProductionOrderStatus {
  Planned = 0,
  FirmPlanned = 1,
  Released = 2,
  Finished = 3,
}
export const productionOrderStatusOptions = mapEnumToOptions(ProductionOrderStatus);

export interface PagedManufacturingInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
}

export interface ManufacturingSetupDto {
  productionOrderNos?: string;
  wipAccountNo?: string;
  capacityAppliedAccountNo?: string;
  varianceAccountNo?: string;
}

export interface WorkCenterDto extends CodeTableDto {
  unitCostPerHour: number;
  capacityHoursPerDay: number;
}

export interface CreateUpdateWorkCenterDto extends CreateUpdateCodeTableDto {
  unitCostPerHour: number;
  capacityHoursPerDay: number;
}

export interface MachineCenterDto extends CodeTableDto {
  workCenterCode?: string;
  unitCostPerHour: number;
}

export interface CreateUpdateMachineCenterDto extends CreateUpdateCodeTableDto {
  workCenterCode?: string;
  unitCostPerHour: number;
}

export interface ProductionBomDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  status: ProductionBomStatus;
}

export interface CreateUpdateProductionBomDto {
  no: string;
  description?: string;
  status: ProductionBomStatus;
}

export interface ProductionBomLineDto extends EntityDto<string> {
  productionBomId?: string;
  lineNo: number;
  type: ProductionBomLineType;
  no?: string;
  description?: string;
  quantityPer: number;
  scrapPct: number;
}

export interface CreateUpdateProductionBomLineDto {
  productionBomId?: string;
  type: ProductionBomLineType;
  no: string;
  description?: string;
  quantityPer: number;
  scrapPct: number;
}

export interface GetProductionBomLineListInput extends PagedManufacturingInput {
  productionBomId?: string;
}

export interface RoutingDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  status: ProductionBomStatus;
}

export interface CreateUpdateRoutingDto {
  no: string;
  description?: string;
  status: ProductionBomStatus;
}

export interface RoutingLineDto extends EntityDto<string> {
  routingId?: string;
  operationNo?: string;
  type: RoutingLineType;
  no?: string;
  description?: string;
  setupTime: number;
  runTime: number;
}

export interface CreateUpdateRoutingLineDto {
  routingId?: string;
  operationNo: string;
  type: RoutingLineType;
  no: string;
  description?: string;
  setupTime: number;
  runTime: number;
}

export interface GetRoutingLineListInput extends PagedManufacturingInput {
  routingId?: string;
}

export interface ProdOrderComponentDto extends EntityDto<string> {
  lineNo: number;
  itemNo?: string;
  description?: string;
  quantityPer: number;
  scrapPct: number;
  expectedQuantity: number;
  actConsumption: number;
  remainingQuantity: number;
  costAmount: number;
}

export interface ProdOrderRoutingLineDto extends EntityDto<string> {
  operationNo?: string;
  type: RoutingLineType;
  no?: string;
  description?: string;
  setupTime: number;
  runTime: number;
  unitCostPerHour: number;
  expectedMinutes: number;
  actualMinutes: number;
  costAmount: number;
}

export interface ProductionOrderDto extends FullAuditedEntityDto<string> {
  no?: string;
  status: ProductionOrderStatus;
  sourceItemNo?: string;
  description?: string;
  quantity: number;
  dueDate?: string;
  locationCode?: string;
  finishedQuantity: number;
  finishedDate?: string;
  materialCost: number;
  capacityCost: number;
  outputCost: number;
  varianceCost: number;
  wip: number;
  components: ProdOrderComponentDto[];
  routingLines: ProdOrderRoutingLineDto[];
}

export interface CreateUpdateProductionOrderDto {
  no?: string;
  status: ProductionOrderStatus;
  sourceItemNo: string;
  description?: string;
  quantity: number;
  dueDate?: string;
  locationCode?: string;
}

export interface GetProductionOrderListInput extends PagedManufacturingInput {
  status?: ProductionOrderStatus;
}

export interface CapacityLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  productionOrderNo?: string;
  operationNo?: string;
  type: RoutingLineType;
  no?: string;
  itemNo?: string;
  postingDate?: string;
  outputQuantity: number;
  minutes: number;
  costAmount: number;
}

export interface GetCapacityLedgerListInput extends PagedManufacturingInput {
  productionOrderNo?: string;
}

export interface PlanningLineDto extends EntityDto<string> {
  itemNo?: string;
  description?: string;
  actionMessage: number;
  replenishmentSystem: ReplenishmentSystem;
  vendorNo?: string;
  locationCode?: string;
  quantity: number;
  dueDate?: string;
  lowLevelCode: number;
  acceptActionMessage: boolean;
  carriedOutNo?: string;
}

export interface UpdatePlanningLineDto {
  quantity: number;
  dueDate?: string;
  acceptActionMessage: boolean;
  vendorNo?: string;
}

export interface CarryOutResultDto {
  purchaseOrderNos: string[];
  productionOrderNos: string[];
}
