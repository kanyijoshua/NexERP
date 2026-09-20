import type { ExportFormat } from './export-format.enum';
import type { EntityFilterOperator } from './entity-filter-operator.enum';
import type { EntityDto } from '@abp/ng.core';

export interface CreateUpdateExportTemplateDto {
  name: string;
  entityName: string;
  fields: string[];
  format: ExportFormat;
  isShared: boolean;
}

export interface DataExportInput {
  entityName: string;
  fields: string[];
  filters: EntityFilterDto[];
  orderBy?: string;
  descending: boolean;
  format: ExportFormat;
  maxResultCount: number;
}

export interface DataPreviewDto {
  totalCount: number;
  fields: ExportableFieldDto[];
  items: object[];
}

export interface DataPreviewInput {
  entityName: string;
  fields: string[];
  filters: EntityFilterDto[];
  orderBy?: string;
  descending: boolean;
  skipCount: number;
  maxResultCount: number;
}

export interface EntityFilterDto {
  field: string;
  operator: EntityFilterOperator;
  value?: string;
}

export interface ExportTemplateDto extends EntityDto<string> {
  name?: string;
  entityName?: string;
  fields: string[];
  format: ExportFormat;
  isShared: boolean;
  ownerUserId?: string;
}

export interface ExportableEntityDto {
  name?: string;
  displayName?: string;
  fieldCount: number;
  isCompanyScoped: boolean;
}

export interface ExportableFieldDto {
  name?: string;
  displayName?: string;
  dataType?: string;
  enumValues: string[];
  includedByDefault: boolean;
}

export interface GetExportTemplatesInput {
  entityName?: string;
}
