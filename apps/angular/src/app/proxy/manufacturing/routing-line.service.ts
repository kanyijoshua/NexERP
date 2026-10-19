import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateRoutingLineDto, GetRoutingLineListInput, RoutingLineDto } from './manufacturing.models';

@Injectable({
  providedIn: 'root',
})
export class RoutingLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateRoutingLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RoutingLineDto>({
      method: 'POST',
      url: '/api/erp/routing-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/routing-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RoutingLineDto>({
      method: 'GET',
      url: `/api/erp/routing-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetRoutingLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<RoutingLineDto>>({
      method: 'GET',
      url: '/api/erp/routing-line',
      params: { routingId: input.routingId, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateRoutingLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RoutingLineDto>({
      method: 'PUT',
      url: `/api/erp/routing-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
