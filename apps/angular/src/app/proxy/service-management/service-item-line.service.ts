import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateServiceItemLineDto, GetServiceOrderLineListInput, ServiceItemLineDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class ServiceItemLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateServiceItemLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceItemLineDto>({
      method: 'POST',
      url: '/api/erp/service-item-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/service-item-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceItemLineDto>({
      method: 'GET',
      url: `/api/erp/service-item-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetServiceOrderLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ServiceItemLineDto>>({
      method: 'GET',
      url: '/api/erp/service-item-line',
      params: { serviceOrderId: input.serviceOrderId, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateServiceItemLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceItemLineDto>({
      method: 'PUT',
      url: `/api/erp/service-item-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
