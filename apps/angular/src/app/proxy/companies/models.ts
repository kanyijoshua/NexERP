import type { FullAuditedEntityDto } from '@abp/ng.core';

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
