import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { GLEntryDocumentType } from '../finance/glentry-document-type.enum';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { EmployeeStatus } from './employee-status.enum';

export interface HumanResourcesSetupDto {
  employeeNos?: string;
  baseUnitOfMeasure?: string;
}

export interface HumanResourceUnitOfMeasureDto extends CodeTableDto {
  qtyPerUnitOfMeasure: number;
}

export interface CreateUpdateHumanResourceUnitOfMeasureDto extends CreateUpdateCodeTableDto {
  qtyPerUnitOfMeasure: number;
}

export interface EmployeePostingGroupDto extends CodeTableDto {
  payablesAccountNo?: string;
}

export interface CreateUpdateEmployeePostingGroupDto extends CreateUpdateCodeTableDto {
  payablesAccountNo: string;
}

export interface CauseOfAbsenceDto extends CodeTableDto {
  unitOfMeasureCode?: string;
}

export interface CreateUpdateCauseOfAbsenceDto extends CreateUpdateCodeTableDto {
  unitOfMeasureCode?: string;
}

export interface EmployeeDto extends FullAuditedEntityDto<string> {
  balance: number;
  no?: string;
  firstName?: string;
  middleName?: string;
  lastName?: string;
  fullName?: string;
  jobTitle?: string;
  address?: string;
  city?: string;
  postCode?: string;
  countryRegionCode?: string;
  phoneNo?: string;
  mobilePhoneNo?: string;
  email?: string;
  companyEmail?: string;
  birthDate?: string;
  socialSecurityNo?: string;
  employmentDate?: string;
  status: EmployeeStatus;
  inactiveDate?: string;
  terminationDate?: string;
  groundsForTermCode?: string;
  emplymtContractCode?: string;
  unionCode?: string;
  employeePostingGroup?: string;
  bankAccountNo?: string;
  iban?: string;
  salespersPurchCode?: string;
  blocked: boolean;
}

export interface CreateUpdateEmployeeDto {
  no?: string;
  firstName: string;
  middleName?: string;
  lastName?: string;
  jobTitle?: string;
  address?: string;
  city?: string;
  postCode?: string;
  countryRegionCode?: string;
  phoneNo?: string;
  mobilePhoneNo?: string;
  email?: string;
  companyEmail?: string;
  birthDate?: string;
  socialSecurityNo?: string;
  employmentDate?: string;
  status: EmployeeStatus;
  inactiveDate?: string;
  terminationDate?: string;
  groundsForTermCode?: string;
  emplymtContractCode?: string;
  unionCode?: string;
  employeePostingGroup?: string;
  bankAccountNo?: string;
  iban?: string;
  salespersPurchCode?: string;
}

export interface GetEmployeeListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  status?: EmployeeStatus;
}

export interface EmployeeAbsenceDto extends FullAuditedEntityDto<string> {
  employeeId?: string;
  employeeNo?: string;
  fromDate?: string;
  toDate?: string;
  causeOfAbsenceCode?: string;
  description?: string;
  quantity: number;
  unitOfMeasureCode?: string;
}

export interface CreateUpdateEmployeeAbsenceDto {
  employeeNo: string;
  fromDate?: string;
  toDate?: string;
  causeOfAbsenceCode?: string;
  description?: string;
  quantity: number;
  unitOfMeasureCode?: string;
}

export interface GetEmployeeAbsenceListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  employeeId?: string;
}

export interface EmployeeLedgerEntryDto extends EntityDto<string> {
  entryNo: number;
  employeeId?: string;
  employeeNo?: string;
  postingDate?: string;
  documentDate?: string;
  documentType: GLEntryDocumentType;
  documentNo?: string;
  description?: string;
  amount: number;
  remainingAmount: number;
  open: boolean;
  closedByEntryNo: number;
  reversed: boolean;
}

export interface GetEmployeeLedgerEntryListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  employeeNo?: string;
  onlyOpen?: boolean;
}
