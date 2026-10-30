import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';

export enum CostTypeKind {
  CostType = 0,
  Heading = 1,
  Total = 2,
}
export const costTypeKindOptions = mapEnumToOptions(CostTypeKind);

export enum CostRegisterSource {
  TransferFromGL = 0,
  Allocation = 1,
}
export const costRegisterSourceOptions = mapEnumToOptions(CostRegisterSource);

export interface ErpPagedListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
}

export interface CostAccountingSetupDto {
  costCenterDimension?: string;
  costObjectDimension?: string;
  lastGLEntryNoTransferred: number;
}

export interface CostTypeDto extends FullAuditedEntityDto<string> {
  no?: string;
  name?: string;
  type: CostTypeKind;
  glAccountRange?: string;
  totaling?: string;
  costCenterCode?: string;
  costObjectCode?: string;
  blocked: boolean;
}

export interface CreateUpdateCostTypeDto {
  no: string;
  name?: string;
  type: CostTypeKind;
  glAccountRange?: string;
  totaling?: string;
  costCenterCode?: string;
  costObjectCode?: string;
  blocked: boolean;
}

export interface CostCenterDto extends CodeTableDto {
  blocked: boolean;
}

export interface CreateUpdateCostCenterDto extends CreateUpdateCodeTableDto {
  blocked: boolean;
}

export interface CostObjectDto extends CodeTableDto {
  blocked: boolean;
}

export interface CreateUpdateCostObjectDto extends CreateUpdateCodeTableDto {
  blocked: boolean;
}

export interface CostEntryDto extends EntityDto<string> {
  entryNo: number;
  costTypeNo?: string;
  costCenterCode?: string;
  costObjectCode?: string;
  postingDate?: string;
  documentNo?: string;
  description?: string;
  amount: number;
  glEntryNo: number;
  glAccountNo?: string;
  allocationCode?: string;
  registerNo: number;
}

export interface GetCostEntryListInput extends ErpPagedListInput {
  costTypeNo?: string;
  costCenterCode?: string;
  costObjectCode?: string;
}

export interface CostRegisterDto extends EntityDto<string> {
  no: number;
  source: CostRegisterSource;
  postingDate?: string;
  fromEntryNo: number;
  toEntryNo: number;
  entryCount: number;
  debitAmount: number;
  createdAt?: string;
}

export interface CostAllocationSourceDto extends FullAuditedEntityDto<string> {
  code?: string;
  description?: string;
  level: number;
  costTypeRange?: string;
  costCenterCode?: string;
  costObjectCode?: string;
  creditToCostTypeNo?: string;
  blocked: boolean;
  totalShare: number;
}

export interface CreateUpdateCostAllocationSourceDto {
  code: string;
  description?: string;
  level: number;
  costTypeRange: string;
  costCenterCode?: string;
  costObjectCode?: string;
  creditToCostTypeNo: string;
  blocked: boolean;
}

export interface CostAllocationTargetDto extends EntityDto<string> {
  costAllocationSourceId?: string;
  lineNo: number;
  costTypeNo?: string;
  costCenterCode?: string;
  costObjectCode?: string;
  share: number;
}

export interface CreateUpdateCostAllocationTargetDto {
  costAllocationSourceId?: string;
  costTypeNo: string;
  costCenterCode?: string;
  costObjectCode?: string;
  share: number;
}

export interface GetCostAllocationTargetListInput extends ErpPagedListInput {
  costAllocationSourceId?: string;
}

export interface AllocateCostsInput {
  fromLevel: number;
  toLevel: number;
  postingDate?: string;
  documentNo: string;
}

export interface GetCostBalancesInput {
  fromDate: string;
  toDate: string;
  byCostObject: boolean;
}

export interface CostBalanceDto {
  costTypeNo?: string;
  costTypeName?: string;
  costCenterCode?: string;
  costObjectCode?: string;
  amount: number;
}
