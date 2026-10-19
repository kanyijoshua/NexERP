import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import { ServiceOrderStatus } from './service-management.models';
import type { CreateUpdateServiceOrderDto, GetServiceOrderListInput, ServiceOrderDto, ServicePostingResultDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class ServiceOrderService {
  apiName = 'Erp';

  create = (input: CreateUpdateServiceOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceOrderDto>({
      method: 'POST',
      url: '/api/erp/service-order',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/service-order/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceOrderDto>({
      method: 'GET',
      url: `/api/erp/service-order/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetServiceOrderListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ServiceOrderDto>>({
      method: 'GET',
      url: '/api/erp/service-order',
      params: { status: input.status, resourceNo: input.resourceNo, dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateServiceOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceOrderDto>({
      method: 'PUT',
      url: `/api/erp/service-order/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  changeStatus = (id: string, input: { status: ServiceOrderStatus }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceOrderDto>({
      method: 'POST',
      url: `/api/erp/service-order/${id}/change-status`,
      body: input,
    },
    { apiName: this.apiName,...config });

  runPosting = (id: string, input: { postingDate?: string }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServicePostingResultDto>({
      method: 'POST',
      url: `/api/erp/service-order/${id}/run-posting`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
