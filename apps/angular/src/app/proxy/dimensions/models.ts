import type { EntityDto } from '@abp/ng.core';

export interface CreateDimensionDto {
  code: string;
  name: string;
  description?: string;
}

export interface CreateDimensionValueDto {
  dimensionId?: string;
  code: string;
  name: string;
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
