import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateProjectDto, ProjectDto, ProjectInvoiceResultDto, ProjectPostingDateInput, ProjectTaskStatisticsDto, ProjectWipResultDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  apiName = 'Erp';

  create = (input: CreateUpdateProjectDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectDto>({
      method: 'POST',
      url: '/api/erp/project',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/project/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectDto>({
      method: 'GET',
      url: `/api/erp/project/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProjectDto>>({
      method: 'GET',
      url: '/api/erp/project',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateProjectDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectDto>({
      method: 'PUT',
      url: `/api/erp/project/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  getStatistics = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectTaskStatisticsDto[]>({
      method: 'GET',
      url: `/api/erp/project/${id}/statistics`,
    },
    { apiName: this.apiName,...config });

  calculateWip = (id: string, input: ProjectPostingDateInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectWipResultDto>({
      method: 'POST',
      url: `/api/erp/project/${id}/calculate-wip`,
      body: input,
    },
    { apiName: this.apiName,...config });

  makeSalesInvoice = (id: string, input: ProjectPostingDateInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectInvoiceResultDto>({
      method: 'POST',
      url: `/api/erp/project/${id}/make-sales-invoice`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
