import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CostAllocationSourceDto, CreateUpdateCostAllocationSourceDto } from './cost-accounting.models';

@Injectable({
  providedIn: 'root',
})
export class CostAllocationSourceService {
  apiName = 'Erp';

  create = (input: CreateUpdateCostAllocationSourceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostAllocationSourceDto>({
      method: 'POST',
      url: '/api/erp/cost-allocation-source',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/cost-allocation-source/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostAllocationSourceDto>({
      method: 'GET',
      url: `/api/erp/cost-allocation-source/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CostAllocationSourceDto>>({
      method: 'GET',
      url: '/api/erp/cost-allocation-source',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCostAllocationSourceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostAllocationSourceDto>({
      method: 'PUT',
      url: `/api/erp/cost-allocation-source/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
