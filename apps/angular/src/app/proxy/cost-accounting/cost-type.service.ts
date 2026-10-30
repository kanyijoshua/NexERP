import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CostTypeDto, CreateUpdateCostTypeDto } from './cost-accounting.models';

@Injectable({
  providedIn: 'root',
})
export class CostTypeService {
  apiName = 'Erp';

  create = (input: CreateUpdateCostTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostTypeDto>({
      method: 'POST',
      url: '/api/erp/cost-type',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/cost-type/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostTypeDto>({
      method: 'GET',
      url: `/api/erp/cost-type/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CostTypeDto>>({
      method: 'GET',
      url: '/api/erp/cost-type',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCostTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostTypeDto>({
      method: 'PUT',
      url: `/api/erp/cost-type/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
