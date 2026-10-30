import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CostAllocationTargetDto, CreateUpdateCostAllocationTargetDto, GetCostAllocationTargetListInput } from './cost-accounting.models';

@Injectable({
  providedIn: 'root',
})
export class CostAllocationTargetService {
  apiName = 'Erp';

  create = (input: CreateUpdateCostAllocationTargetDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostAllocationTargetDto>({
      method: 'POST',
      url: '/api/erp/cost-allocation-target',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/cost-allocation-target/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostAllocationTargetDto>({
      method: 'GET',
      url: `/api/erp/cost-allocation-target/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCostAllocationTargetListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CostAllocationTargetDto>>({
      method: 'GET',
      url: '/api/erp/cost-allocation-target',
      params: { dynamicFilter: input.dynamicFilter, costAllocationSourceId: input.costAllocationSourceId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCostAllocationTargetDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostAllocationTargetDto>({
      method: 'PUT',
      url: `/api/erp/cost-allocation-target/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
