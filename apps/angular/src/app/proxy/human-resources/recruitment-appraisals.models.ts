import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { ErpPagedListInput } from '../cost-accounting/cost-accounting.models';
import type { EntityDto, FullAuditedEntityDto } from '@abp/ng.core';
import { mapEnumToOptions } from '@abp/ng.core';

export enum JobVacancyStatus {
  Draft = 0,
  Open = 1,
  Closed = 2,
}
export const jobVacancyStatusOptions = mapEnumToOptions(JobVacancyStatus);

export enum ApplicantStage {
  New = 0,
  Screening = 1,
  Interview = 2,
  Offer = 3,
  Hired = 4,
  Rejected = 5,
}
export const applicantStageOptions = mapEnumToOptions(ApplicantStage);

export enum AppraisalStatus {
  Draft = 0,
  SelfReview = 1,
  ManagerReview = 2,
  Completed = 3,
}
export const appraisalStatusOptions = mapEnumToOptions(AppraisalStatus);

export interface JobVacancyDto extends FullAuditedEntityDto<string> {
  no?: string;
  title?: string;
  department?: string;
  description?: string;
  positions: number;
  hiredCount: number;
  openingDate?: string;
  closingDate?: string;
  status: JobVacancyStatus;
}

export interface CreateUpdateJobVacancyDto {
  no?: string;
  title: string;
  department?: string;
  description?: string;
  positions: number;
  openingDate?: string;
  closingDate?: string;
}

export interface GetRecruitmentListInput extends ErpPagedListInput {
  filter?: string;
  vacancyNo?: string;
}

export interface JobApplicantDto extends FullAuditedEntityDto<string> {
  no?: string;
  vacancyNo?: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  email?: string;
  phoneNo?: string;
  appliedDate?: string;
  stage: ApplicantStage;
  rating: number;
  expectedSalary: number;
  notes?: string;
  employeeNo?: string;
}

export interface CreateUpdateJobApplicantDto {
  no?: string;
  vacancyNo: string;
  firstName: string;
  lastName?: string;
  email?: string;
  phoneNo?: string;
  appliedDate?: string;
  rating: number;
  expectedSalary: number;
  notes?: string;
}

export interface ChangeApplicantStageInput {
  stage: ApplicantStage;
}

export interface HireApplicantInput {
  employmentDate?: string;
  employeeNo?: string;
}

export interface AppraisalPeriodDto extends CodeTableDto {
  startingDate?: string;
  endingDate?: string;
  closed: boolean;
}

export interface CreateUpdateAppraisalPeriodDto extends CreateUpdateCodeTableDto {
  startingDate?: string;
  endingDate?: string;
  closed: boolean;
}

export interface AppraisalDto extends FullAuditedEntityDto<string> {
  no?: string;
  employeeNo?: string;
  employeeName?: string;
  periodCode?: string;
  reviewerEmployeeNo?: string;
  status: AppraisalStatus;
  overallScore: number;
  employeeComment?: string;
  reviewerComment?: string;
  completedDate?: string;
}

export interface CreateUpdateAppraisalDto {
  no?: string;
  employeeNo: string;
  periodCode: string;
  reviewerEmployeeNo?: string;
  employeeComment?: string;
  reviewerComment?: string;
}

export interface GetAppraisalListInput extends ErpPagedListInput {
  employeeNo?: string;
  periodCode?: string;
}

export interface AppraisalLineDto extends EntityDto<string> {
  appraisalId?: string;
  lineNo: number;
  objective?: string;
  weight: number;
  target?: string;
  selfScore: number;
  managerScore: number;
  comment?: string;
}

export interface CreateUpdateAppraisalLineDto {
  appraisalId?: string;
  objective: string;
  weight: number;
  target?: string;
  selfScore: number;
  managerScore: number;
  comment?: string;
}

export interface GetAppraisalLineListInput extends ErpPagedListInput {
  appraisalId?: string;
}
