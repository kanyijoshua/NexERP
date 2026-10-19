import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateResourceDto, ResourceDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class ResourceService {
  apiName = 'Erp';

  create = (input: CreateUpdateResourceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ResourceDto>({
      method: 'POST',
      url: '/api/erp/resource',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/resource/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ResourceDto>({
      method: 'GET',
      url: `/api/erp/resource/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ResourceDto>>({
      method: 'GET',
      url: '/api/erp/resource',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateResourceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ResourceDto>({
      method: 'PUT',
      url: `/api/erp/resource/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
