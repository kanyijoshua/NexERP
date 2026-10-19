import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import { ProductionOrderStatus } from './manufacturing.models';
import type { CreateUpdateProductionOrderDto, GetProductionOrderListInput, ProductionOrderDto } from './manufacturing.models';

@Injectable({
  providedIn: 'root',
})
export class ProductionOrderService {
  apiName = 'Erp';

  create = (input: CreateUpdateProductionOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionOrderDto>({
      method: 'POST',
      url: '/api/erp/production-order',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/production-order/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionOrderDto>({
      method: 'GET',
      url: `/api/erp/production-order/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetProductionOrderListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProductionOrderDto>>({
      method: 'GET',
      url: '/api/erp/production-order',
      params: { status: input.status, dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateProductionOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionOrderDto>({
      method: 'PUT',
      url: `/api/erp/production-order/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  refresh = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionOrderDto>({
      method: 'POST',
      url: `/api/erp/production-order/${id}/refresh`,
    },
    { apiName: this.apiName,...config });

  changeStatus = (id: string, input: { status: ProductionOrderStatus }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionOrderDto>({
      method: 'POST',
      url: `/api/erp/production-order/${id}/change-status`,
      body: input,
    },
    { apiName: this.apiName,...config });

  consume = (id: string, input: { postingDate?: string; lines: { componentId: string; quantity: number }[] }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionOrderDto>({
      method: 'POST',
      url: `/api/erp/production-order/${id}/consume`,
      body: input,
    },
    { apiName: this.apiName,...config });

  reportOutput = (id: string, input: { postingDate?: string; quantity: number; capacity: { operationNo: string; minutes: number }[] }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionOrderDto>({
      method: 'POST',
      url: `/api/erp/production-order/${id}/report-output`,
      body: input,
    },
    { apiName: this.apiName,...config });

  finish = (id: string, input: { postingDate?: string }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionOrderDto>({
      method: 'POST',
      url: `/api/erp/production-order/${id}/finish`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
