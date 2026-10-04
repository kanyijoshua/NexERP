import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { EmployeeQualificationType } from './employee-qualification-type.enum';
import type { EmployeeStatus } from './employee-status.enum';
import type { HumanResourcesCommentTableName } from './human-resources-comment-table-name.enum';

export interface EmployeeRelativeDto extends FullAuditedEntityDto<string> {
  employeeNo?: string;
  lineNo: number;
  relativeCode?: string;
  firstName?: string;
  middleName?: string;
  lastName?: string;
  birthDate?: string;
  phoneNo?: string;
  relativesEmployeeNo?: string;
}

export interface CreateUpdateEmployeeRelativeDto {
  employeeNo: string;
  lineNo: number;
  relativeCode?: string;
  firstName?: string;
  middleName?: string;
  lastName?: string;
  birthDate?: string;
  phoneNo?: string;
  relativesEmployeeNo?: string;
}

export interface GetEmployeeRelativeListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  employeeNo?: string;
}

export interface EmployeeQualificationDto extends FullAuditedEntityDto<string> {
  employeeNo?: string;
  lineNo: number;
  qualificationCode?: string;
  fromDate?: string;
  toDate?: string;
  type: EmployeeQualificationType;
  description?: string;
  institutionCompany?: string;
  cost: number;
  courseGrade?: string;
  employeeStatus: EmployeeStatus;
  expirationDate?: string;
}

export interface CreateUpdateEmployeeQualificationDto {
  employeeNo: string;
  lineNo: number;
  qualificationCode?: string;
  fromDate?: string;
  toDate?: string;
  type: EmployeeQualificationType;
  description?: string;
  institutionCompany?: string;
  cost: number;
  courseGrade?: string;
  employeeStatus: EmployeeStatus;
  expirationDate?: string;
}

export interface GetEmployeeQualificationListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  employeeNo?: string;
}

export interface MiscArticleInformationDto extends FullAuditedEntityDto<string> {
  employeeNo?: string;
  miscArticleCode?: string;
  lineNo: number;
  description?: string;
  fromDate?: string;
  toDate?: string;
  inUse: boolean;
  serialNo?: string;
}

export interface CreateUpdateMiscArticleInformationDto {
  employeeNo: string;
  miscArticleCode: string;
  lineNo: number;
  description?: string;
  fromDate?: string;
  toDate?: string;
  inUse: boolean;
  serialNo?: string;
}

export interface GetMiscArticleInformationListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  employeeNo?: string;
}

export interface ConfidentialInformationDto extends FullAuditedEntityDto<string> {
  employeeNo?: string;
  confidentialCode?: string;
  lineNo: number;
  description?: string;
}

export interface CreateUpdateConfidentialInformationDto {
  employeeNo: string;
  confidentialCode: string;
  lineNo: number;
  description?: string;
}

export interface GetConfidentialInformationListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  employeeNo?: string;
}

export interface AlternativeAddressDto extends FullAuditedEntityDto<string> {
  employeeNo?: string;
  code?: string;
  name?: string;
  name2?: string;
  address?: string;
  address2?: string;
  city?: string;
  postCode?: string;
  county?: string;
  phoneNo?: string;
  faxNo?: string;
  email?: string;
  countryRegionCode?: string;
}

export interface CreateUpdateAlternativeAddressDto {
  employeeNo: string;
  code: string;
  name?: string;
  name2?: string;
  address?: string;
  address2?: string;
  city?: string;
  postCode?: string;
  county?: string;
  phoneNo?: string;
  faxNo?: string;
  email?: string;
  countryRegionCode?: string;
}

export interface GetAlternativeAddressListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  employeeNo?: string;
}

export interface HumanResourceCommentLineDto extends FullAuditedEntityDto<string> {
  tableName: HumanResourcesCommentTableName;
  no?: string;
  tableLineNo: number;
  lineNo: number;
  alternativeAddressCode?: string;
  date?: string;
  code?: string;
  comment?: string;
}

export interface CreateUpdateHumanResourceCommentLineDto {
  tableName: HumanResourcesCommentTableName;
  no: string;
  tableLineNo: number;
  lineNo: number;
  alternativeAddressCode?: string;
  date?: string;
  code?: string;
  comment?: string;
}

export interface GetHumanResourceCommentLineListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  no?: string;
}
