import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CostCenterDto, CreateUpdateCostCenterDto } from './cost-accounting.models';

@Injectable({
  providedIn: 'root',
})
export class CostCenterService {
  apiName = 'Erp';

  create = (input: CreateUpdateCostCenterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostCenterDto>({
      method: 'POST',
      url: '/api/erp/cost-center',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/cost-center/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostCenterDto>({
      method: 'GET',
      url: `/api/erp/cost-center/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CostCenterDto>>({
      method: 'GET',
      url: '/api/erp/cost-center',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCostCenterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostCenterDto>({
      method: 'PUT',
      url: `/api/erp/cost-center/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
