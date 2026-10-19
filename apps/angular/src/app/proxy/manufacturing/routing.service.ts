import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateRoutingDto, RoutingDto } from './manufacturing.models';

@Injectable({
  providedIn: 'root',
})
export class RoutingService {
  apiName = 'Erp';

  create = (input: CreateUpdateRoutingDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RoutingDto>({
      method: 'POST',
      url: '/api/erp/routing',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/routing/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RoutingDto>({
      method: 'GET',
      url: `/api/erp/routing/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<RoutingDto>>({
      method: 'GET',
      url: '/api/erp/routing',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateRoutingDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RoutingDto>({
      method: 'PUT',
      url: `/api/erp/routing/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
