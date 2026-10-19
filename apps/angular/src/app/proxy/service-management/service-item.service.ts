import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateServiceItemDto, ServiceItemDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class ServiceItemService {
  apiName = 'Erp';

  create = (input: CreateUpdateServiceItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceItemDto>({
      method: 'POST',
      url: '/api/erp/service-item',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/service-item/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceItemDto>({
      method: 'GET',
      url: `/api/erp/service-item/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ServiceItemDto>>({
      method: 'GET',
      url: '/api/erp/service-item',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateServiceItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceItemDto>({
      method: 'PUT',
      url: `/api/erp/service-item/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
