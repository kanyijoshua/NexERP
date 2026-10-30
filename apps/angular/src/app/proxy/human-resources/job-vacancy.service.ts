import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateJobVacancyDto, GetRecruitmentListInput, JobVacancyDto } from './recruitment-appraisals.models';

@Injectable({
  providedIn: 'root',
})
export class JobVacancyService {
  apiName = 'Erp';

  close = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobVacancyDto>({
      method: 'POST',
      url: `/api/erp/job-vacancy/${id}/close`,
    },
    { apiName: this.apiName,...config });

  create = (input: CreateUpdateJobVacancyDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobVacancyDto>({
      method: 'POST',
      url: '/api/erp/job-vacancy',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/job-vacancy/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobVacancyDto>({
      method: 'GET',
      url: `/api/erp/job-vacancy/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetRecruitmentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<JobVacancyDto>>({
      method: 'GET',
      url: '/api/erp/job-vacancy',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, vacancyNo: input.vacancyNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  open = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobVacancyDto>({
      method: 'POST',
      url: `/api/erp/job-vacancy/${id}/open`,
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateJobVacancyDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JobVacancyDto>({
      method: 'PUT',
      url: `/api/erp/job-vacancy/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
