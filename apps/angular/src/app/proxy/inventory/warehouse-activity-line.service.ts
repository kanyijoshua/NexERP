import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateWarehouseActivityLineDto, GetWarehouseActivityLineListInput, WarehouseActivityLineDto } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class WarehouseActivityLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateWarehouseActivityLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseActivityLineDto>({
      method: 'POST',
      url: '/api/erp/warehouse-activity-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/warehouse-activity-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseActivityLineDto>({
      method: 'GET',
      url: `/api/erp/warehouse-activity-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetWarehouseActivityLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WarehouseActivityLineDto>>({
      method: 'GET',
      url: '/api/erp/warehouse-activity-line',
      params: { dynamicFilter: input.dynamicFilter, warehouseActivityHeaderId: input.warehouseActivityHeaderId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateWarehouseActivityLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseActivityLineDto>({
      method: 'PUT',
      url: `/api/erp/warehouse-activity-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
