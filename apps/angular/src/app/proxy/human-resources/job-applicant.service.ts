import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ChangeApplicantStageInput, CreateUpdateJobApplicantDto, GetRecruitmentListInput, HireApplicantInput, JobApplicantDto } from './recruitment-appraisals.models';

@Injectable({
  providedIn: 'root',
})
export class JobApplicantService {
  apiName = 'Erp';

  changeStage = (id: string, input: ChangeApplicantStageInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobApplicantDto>({
      method: 'POST',
      url: `/api/erp/job-applicant/${id}/change-stage`,
      body: input,
    },
    { apiName: this.apiName,...config });

  create = (input: CreateUpdateJobApplicantDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobApplicantDto>({
      method: 'POST',
      url: '/api/erp/job-applicant',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/job-applicant/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobApplicantDto>({
      method: 'GET',
      url: `/api/erp/job-applicant/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetRecruitmentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<JobApplicantDto>>({
      method: 'GET',
      url: '/api/erp/job-applicant',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, vacancyNo: input.vacancyNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  hire = (id: string, input: HireApplicantInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobApplicantDto>({
      method: 'POST',
      url: `/api/erp/job-applicant/${id}/hire`,
      body: input,
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateJobApplicantDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobApplicantDto>({
      method: 'PUT',
      url: `/api/erp/job-applicant/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
