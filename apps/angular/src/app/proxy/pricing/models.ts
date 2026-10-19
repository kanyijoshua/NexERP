import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { PriceAssetType, PriceListStatus, PriceSourceType, PriceType } from './pricing.enums';

export interface PriceListDto extends FullAuditedEntityDto<string> {
  code?: string;
  description?: string;
  priceType: PriceType;
  sourceType: PriceSourceType;
  sourceNo?: string;
  currencyCode?: string;
  startingDate?: string;
  endingDate?: string;
  status: PriceListStatus;
}

export interface CreateUpdatePriceListDto {
  code: string;
  description?: string;
  priceType: PriceType;
  sourceType: PriceSourceType;
  sourceNo?: string;
  currencyCode?: string;
  startingDate?: string;
  endingDate?: string;
  status: PriceListStatus;
}

export interface GetPriceListListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  priceType?: PriceType;
  status?: PriceListStatus;
}

export interface PriceListLineDto extends EntityDto<string> {
  priceListId?: string;
  lineNo: number;
  assetType: PriceAssetType;
  assetNo?: string;
  description?: string;
  unitOfMeasureCode?: string;
  minimumQuantity: number;
  unitPrice: number;
  lineDiscountPercent: number;
}

export interface CreateUpdatePriceListLineDto {
  priceListId: string;
  assetType: PriceAssetType;
  assetNo: string;
  description?: string;
  unitOfMeasureCode?: string;
  minimumQuantity: number;
  unitPrice: number;
  lineDiscountPercent: number;
}

export interface GetPriceListLineListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  priceListId?: string;
  filter?: string;
}

export interface GetPriceInput {
  priceType: PriceType;
  partyNo?: string;
  itemNo: string;
  unitOfMeasureCode?: string;
  quantity: number;
  currencyCode?: string;
  date?: string;
}

export interface PriceQuoteDto {
  unitPrice: number;
  lineDiscountPercent: number;
  priceListCode?: string;
}

export interface CustomerPriceGroupDto extends CodeTableDto {
  allowLineDiscount: boolean;
}

export interface CreateUpdateCustomerPriceGroupDto extends CreateUpdateCodeTableDto {
  allowLineDiscount: boolean;
}
