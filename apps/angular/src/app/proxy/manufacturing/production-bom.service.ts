import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateProductionBomDto, ProductionBomDto } from './manufacturing.models';

@Injectable({
  providedIn: 'root',
})
export class ProductionBomService {
  apiName = 'Erp';

  create = (input: CreateUpdateProductionBomDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionBomDto>({
      method: 'POST',
      url: '/api/erp/production-bom',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/production-bom/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionBomDto>({
      method: 'GET',
      url: `/api/erp/production-bom/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProductionBomDto>>({
      method: 'GET',
      url: '/api/erp/production-bom',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateProductionBomDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionBomDto>({
      method: 'PUT',
      url: `/api/erp/production-bom/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
