import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CostObjectDto, CreateUpdateCostObjectDto } from './cost-accounting.models';

@Injectable({
  providedIn: 'root',
})
export class CostObjectService {
  apiName = 'Erp';

  create = (input: CreateUpdateCostObjectDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostObjectDto>({
      method: 'POST',
      url: '/api/erp/cost-object',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/cost-object/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostObjectDto>({
      method: 'GET',
      url: `/api/erp/cost-object/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CostObjectDto>>({
      method: 'GET',
      url: '/api/erp/cost-object',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCostObjectDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostObjectDto>({
      method: 'PUT',
      url: `/api/erp/cost-object/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
