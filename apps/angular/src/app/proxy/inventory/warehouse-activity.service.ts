import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateWarehouseActivityDto, GetWarehouseActivityListInput, MakeWarehouseDocumentInput, RegisterWarehouseActivityInput, WarehouseActivityDto } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class WarehouseActivityService {
  apiName = 'Erp';

  create = (input: CreateUpdateWarehouseActivityDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseActivityDto>({
      method: 'POST',
      url: '/api/erp/warehouse-activity',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/warehouse-activity/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseActivityDto>({
      method: 'GET',
      url: `/api/erp/warehouse-activity/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetWarehouseActivityListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WarehouseActivityDto>>({
      method: 'GET',
      url: '/api/erp/warehouse-activity',
      params: { dynamicFilter: input.dynamicFilter, type: input.type, status: input.status, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  makePick = (input: MakeWarehouseDocumentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseActivityDto>({
      method: 'POST',
      url: '/api/erp/warehouse-activity/make-pick',
      body: input,
    },
    { apiName: this.apiName,...config });

  makePutAway = (input: MakeWarehouseDocumentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseActivityDto>({
      method: 'POST',
      url: '/api/erp/warehouse-activity/make-put-away',
      body: input,
    },
    { apiName: this.apiName,...config });

  register = (id: string, input: RegisterWarehouseActivityInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseActivityDto>({
      method: 'POST',
      url: `/api/erp/warehouse-activity/${id}/register`,
      body: input,
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateWarehouseActivityDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseActivityDto>({
      method: 'PUT',
      url: `/api/erp/warehouse-activity/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
