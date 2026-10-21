import type { EntityDto } from '@abp/ng.core';

export interface CreateDimensionDto {
  code: string;
  name: string;
  description?: string;
  blocked?: boolean;
}

export interface UpdateDimensionDto {
  name: string;
  description?: string;
  blocked?: boolean;
}

export interface CreateDimensionValueDto {
  dimensionId?: string;
  code: string;
  name: string;
  blocked?: boolean;
}

export interface UpdateDimensionValueDto {
  name: string;
  blocked?: boolean;
}

export interface DimensionDto extends EntityDto<string> {
  code?: string;
  name?: string;
  description?: string;
  blocked: boolean;
}

export interface DimensionValueDto extends EntityDto<string> {
  dimensionId?: string;
  dimensionCode?: string;
  code?: string;
  name?: string;
  blocked: boolean;
}

export interface ShortcutDimensionSlotDto {
  slotNumber: number;
  isGlobal: boolean;
  dimensionCode?: string;
  dimensionName?: string;
  values: DimensionValueDto[];
}

export interface DimensionSetEntryDto {
  dimensionCode?: string;
  dimensionValueCode?: string;
}

export interface DimensionSetDto {
  dimensionSetId?: string;
  shortcutDimension1Code?: string;
  shortcutDimension2Code?: string;
  shortcutDimension3Code?: string;
  shortcutDimension4Code?: string;
  shortcutDimension5Code?: string;
  shortcutDimension6Code?: string;
  shortcutDimension7Code?: string;
  shortcutDimension8Code?: string;
  entries: DimensionSetEntryDto[];
}

export interface ResolveDimensionSetInput {
  existingDimensionSetId?: string;
  shortcutDimension1Code?: string;
  shortcutDimension2Code?: string;
  shortcutDimension3Code?: string;
  shortcutDimension4Code?: string;
  shortcutDimension5Code?: string;
  shortcutDimension6Code?: string;
  shortcutDimension7Code?: string;
  shortcutDimension8Code?: string;
  entries?: Record<string, string>;
}
