import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateProjectTaskDto, GetProjectTaskListInput, ProjectTaskDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class ProjectTaskService {
  apiName = 'Erp';

  create = (input: CreateUpdateProjectTaskDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectTaskDto>({
      method: 'POST',
      url: '/api/erp/project-task',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/project-task/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectTaskDto>({
      method: 'GET',
      url: `/api/erp/project-task/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetProjectTaskListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProjectTaskDto>>({
      method: 'GET',
      url: '/api/erp/project-task',
      params: { projectId: input.projectId, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateProjectTaskDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectTaskDto>({
      method: 'PUT',
      url: `/api/erp/project-task/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
