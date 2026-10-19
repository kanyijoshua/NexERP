import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateProjectPlanningLineDto, GetProjectPlanningLineListInput, ProjectPlanningLineDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class ProjectPlanningLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateProjectPlanningLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectPlanningLineDto>({
      method: 'POST',
      url: '/api/erp/project-planning-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/project-planning-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectPlanningLineDto>({
      method: 'GET',
      url: `/api/erp/project-planning-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetProjectPlanningLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProjectPlanningLineDto>>({
      method: 'GET',
      url: '/api/erp/project-planning-line',
      params: { projectNo: input.projectNo, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateProjectPlanningLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectPlanningLineDto>({
      method: 'PUT',
      url: `/api/erp/project-planning-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
