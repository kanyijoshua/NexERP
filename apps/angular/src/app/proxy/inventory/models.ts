import type { ItemType } from './item-type.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ItemLedgerEntryType } from './item-ledger-entry-type.enum';

export interface CreateUpdateItemCategoryDto {
  code?: string;
  description?: string;
  parentCategoryId?: string;
  parentCategoryCode?: string;
}

export interface CreateUpdateItemDto {
  no?: string;
  description?: string;
  type: ItemType;
  baseUnitOfMeasureCode?: string;
  itemCategoryId?: string;
  itemCategoryCode?: string;
  unitPrice: number;
  unitCost: number;
  genProdPostingGroup?: string;
  inventoryPostingGroup?: string;
}

export interface CreateUpdateUnitOfMeasureDto {
  code?: string;
  description?: string;
}

export interface GetItemLedgerEntryListInput extends PagedAndSortedResultRequestDto {
  itemId?: string;
  documentNo?: string;
  entryType?: ItemLedgerEntryType;
}

export interface GetItemListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  type?: ItemType;
  itemCategoryId?: string;
}

export interface ItemCategoryDto extends FullAuditedEntityDto<string> {
  code?: string;
  description?: string;
  parentCategoryId?: string;
  parentCategoryCode?: string;
}

export interface ItemDto extends FullAuditedEntityDto<string> {
  no?: string;
  description?: string;
  type: ItemType;
  baseUnitOfMeasureCode?: string;
  itemCategoryId?: string;
  itemCategoryCode?: string;
  unitPrice: number;
  unitCost: number;
  inventory: number;
  genProdPostingGroup?: string;
  inventoryPostingGroup?: string;
  blocked: boolean;
}

export interface ItemLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  itemId?: string;
  itemNo?: string;
  postingDate?: string;
  entryType: ItemLedgerEntryType;
  documentNo?: string;
  quantity: number;
  unitCost: number;
  salesAmount: number;
  locationCode?: string;
}

export interface UnitOfMeasureDto extends FullAuditedEntityDto<string> {
  code?: string;
  description?: string;
}
