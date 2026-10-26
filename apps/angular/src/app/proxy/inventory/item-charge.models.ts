import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { ItemChargeAllocationMethod, ItemChargeSourceType } from './item-charge.enums';

export interface ItemChargeDto extends CodeTableDto {
  genProdPostingGroup?: string;
  vatProdPostingGroup?: string;
}

export interface CreateUpdateItemChargeDto extends CreateUpdateCodeTableDto {
  genProdPostingGroup?: string;
  vatProdPostingGroup?: string;
}

export interface ItemChargeAssignmentDto extends FullAuditedEntityDto<string> {
  sourceType: ItemChargeSourceType;
  sourceDocumentNo?: string;
  sourceLineId?: string;
  sourceLineNo: number;
  itemChargeNo?: string;
  appliesToEntryId?: string;
  appliesToEntryNo: number;
  appliesToDocumentNo?: string;
  itemNo?: string;
  description?: string;
  qtyToAssign: number;
  qtyAssigned: number;
  unitCost: number;
  amountToAssign: number;
  posted: boolean;
  amountPostedLcy: number;
}

export interface CreateItemChargeAssignmentDto {
  sourceType: ItemChargeSourceType;
  sourceLineId: string;
  appliesToEntryId: string;
  qtyToAssign: number;
  amountToAssign?: number;
}

export interface UpdateItemChargeAssignmentDto {
  qtyToAssign: number;
  amountToAssign?: number;
}

export interface GetItemChargeAssignmentListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  sourceType?: ItemChargeSourceType;
  sourceLineId?: string;
  sourceDocumentNo?: string;
  posted?: boolean;
}

export interface GetAssignableEntriesInput {
  sourceType: ItemChargeSourceType;
  sourceLineId: string;
  filter?: string;
  maxResultCount?: number;
}

export interface SuggestItemChargeAllocationInput {
  sourceType: ItemChargeSourceType;
  sourceLineId: string;
  method: ItemChargeAllocationMethod;
  targetItemLedgerEntryIds: string[];
}
