import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateServiceLineDto, GetServiceOrderLineListInput, ServiceLineDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class ServiceLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateServiceLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceLineDto>({
      method: 'POST',
      url: '/api/erp/service-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/service-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceLineDto>({
      method: 'GET',
      url: `/api/erp/service-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetServiceOrderLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ServiceLineDto>>({
      method: 'GET',
      url: '/api/erp/service-line',
      params: { serviceOrderId: input.serviceOrderId, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateServiceLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceLineDto>({
      method: 'PUT',
      url: `/api/erp/service-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
