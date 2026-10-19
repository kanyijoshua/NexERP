import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateProjectPostingGroupDto, ProjectPostingGroupDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class ProjectPostingGroupService {
  apiName = 'Erp';

  create = (input: CreateUpdateProjectPostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectPostingGroupDto>({
      method: 'POST',
      url: '/api/erp/project-posting-group',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/project-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectPostingGroupDto>({
      method: 'GET',
      url: `/api/erp/project-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProjectPostingGroupDto>>({
      method: 'GET',
      url: '/api/erp/project-posting-group',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateProjectPostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectPostingGroupDto>({
      method: 'PUT',
      url: `/api/erp/project-posting-group/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
