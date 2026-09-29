import type { EntityDto, PagedResultRequestDto } from '@abp/ng.core';
import type { EntityFilterDto } from '../exporting/models';
import type { ConfigLineStatus } from './config-line-status.enum';
import type { ConfigLineType } from './config-line-type.enum';
import type { ConfigPackageFileFormat } from './config-package-file-format.enum';

export interface ConfigFieldInfoDto {
  name?: string;
  displayName?: string;
  dataType?: string;
  enumValues: string[];
  isKey: boolean;
  isRequired: boolean;
  maxLength?: number;
  relatedTable?: string;
}

export interface ConfigFieldMappingDto {
  oldValue?: string;
  newValue?: string;
}

export interface ConfigLineDto extends EntityDto<string> {
  lineType: ConfigLineType;
  name?: string;
  entityName?: string;
  packageCode?: string;
  status: ConfigLineStatus;
  responsibleUserName?: string;
  comments?: string;
  sortOrder: number;
  noOfRecords?: number;
}

export interface ConfigPackageDetailDto extends ConfigPackageDto {
  tables: ConfigPackageTableDto[];
}

export interface ConfigPackageDto extends EntityDto<string> {
  code?: string;
  packageName?: string;
  productVersion?: string;
  lastImportedTime?: string;
  lastAppliedTime?: string;
  noOfTables: number;
  noOfRecords: number;
  noOfErrors: number;
}

export interface ConfigPackageErrorDto {
  tableId?: string;
  entityName?: string;
  recordId?: string;
  recordNo: number;
  fieldName?: string;
  errorText?: string;
}

export interface ConfigPackageFieldDto extends EntityDto<string> {
  fieldName?: string;
  displayName?: string;
  dataType?: string;
  includeField: boolean;
  validateField: boolean;
  processingOrder: number;
  primaryKey: boolean;
  relatedTable?: string;
  mappings: ConfigFieldMappingDto[];
}

export interface ConfigPackageRecordDto extends EntityDto<string> {
  recordNo: number;
  values: Record<string, string>;
  invalid: boolean;
  errors: ConfigPackageErrorDto[];
}

export interface ConfigPackageRunResultDto {
  tables: ConfigTableRunResultDto[];
  inserted: number;
  modified: number;
  errors: number;
}

export interface ConfigPackageTableDto extends EntityDto<string> {
  entityName?: string;
  displayName?: string;
  processingOrder: number;
  deleteRecordsBeforeProcessing: boolean;
  dataTemplateCode?: string;
  filters: EntityFilterDto[];
  noOfRecords: number;
  noOfErrors: number;
  fields: ConfigPackageFieldDto[];
}

export interface ConfigPackageTableInput {
  tableId?: string;
}

export interface ConfigPackageTablesInput {
  tableIds: string[];
}

export interface ConfigTableInfoDto {
  entityName?: string;
  displayName?: string;
  area?: string;
  keyFields: string[];
  parentEntity?: string;
  relatedTables: string[];
  canWrite: boolean;
}

export interface ConfigTableRunResultDto {
  tableId?: string;
  entityName?: string;
  inserted: number;
  modified: number;
  deleted: number;
  errors: number;
}

export interface ConfigTemplateDto extends EntityDto<string> {
  code?: string;
  description?: string;
  entityName?: string;
  enabled: boolean;
  lines: ConfigTemplateLineDto[];
}

export interface ConfigTemplateLineDto {
  fieldName: string;
  defaultValue?: string;
  mandatory: boolean;
}

export interface CreateConfigLineDto {
  lineType: ConfigLineType;
  name: string;
  entityName?: string;
  packageCode?: string;
  status: ConfigLineStatus;
  responsibleUserName?: string;
  comments?: string;
}

export interface CreateConfigPackageDto {
  code: string;
  packageName: string;
  productVersion?: string;
}

export interface CreateUpdateConfigTemplateDto {
  code: string;
  description?: string;
  entityName: string;
  enabled: boolean;
  lines: ConfigTemplateLineDto[];
}

export interface ExportConfigPackageInput {
  format: ConfigPackageFileFormat;
}

export interface GetConfigPackageRecordsInput extends PagedResultRequestDto {
  packageId?: string;
  tableId?: string;
  errorsOnly: boolean;
}

export interface GetConfigPackagesInput {
  filter?: string;
}

export interface GetConfigTemplatesInput {
  entityName?: string;
}

export interface ImportColumnDto {
  index: number;
  header?: string;
  fieldName?: string;
}

export interface ImportConfigDataResultDto {
  noOfRecords: number;
}

export interface ImportErrorDto {
  rowNo: number;
  fieldName?: string;
  message?: string;
}

export interface ImportFileInput extends UploadFileInput {
  sheetName?: string;
  separator?: string;
  hasHeaders: boolean;
  entityName?: string;
}

export interface ImportFilePreviewDto {
  sheets: string[];
  sheetName?: string;
  columns: ImportColumnDto[];
  sampleRows: string[][];
  totalRows: number;
}

export interface ImportResultDto {
  dryRun: boolean;
  succeeded: boolean;
  totalRows: number;
  inserted: number;
  modified: number;
  errors: ImportErrorDto[];
}

export interface IncludeConfigTablesInput {
  entityNames: string[];
  includeRelatedTables: boolean;
}

export interface MoveConfigLineInput {
  up: boolean;
}

export interface RunImportInput extends ImportFileInput {
  columns: ImportColumnDto[];
  dataTemplateCode?: string;
}

export interface UpdateConfigLineDto {
  name: string;
  packageCode?: string;
  status: ConfigLineStatus;
  responsibleUserName?: string;
  comments?: string;
}

export interface UpdateConfigPackageDto {
  packageName: string;
  productVersion?: string;
}

export interface UpdateConfigPackageFieldDto {
  fieldName: string;
  includeField: boolean;
  validateField: boolean;
  processingOrder: number;
  mappings: ConfigFieldMappingDto[];
}

export interface UpdateConfigPackageRecordDto {
  values: Record<string, string>;
}

export interface UpdateConfigPackageTableDto {
  tableId?: string;
  processingOrder: number;
  deleteRecordsBeforeProcessing: boolean;
  dataTemplateCode?: string;
  filters: EntityFilterDto[];
  fields: UpdateConfigPackageFieldDto[];
}

export interface UploadFileInput {
  fileName: string;
  contentBase64: string;
}
