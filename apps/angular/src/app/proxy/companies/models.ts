import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CompanyDto extends FullAuditedEntityDto<string> {
  name?: string;
  displayName?: string;
  evaluationCompany: boolean;
  isDefault: boolean;
}

export interface CopyCompanyInput {
  sourceCompanyId?: string;
  newCompanyName: string;
  newDisplayName?: string;
}

export interface CreateCompanyDto {
  name: string;
  displayName: string;
  evaluationCompany: boolean;
}

export interface CodeTableDto extends FullAuditedEntityDto<string> {
  code?: string;
  description?: string;
}

export interface CreateUpdateCodeTableDto {
  code: string;
  description?: string;
}

export interface GetCodeTableListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
}
