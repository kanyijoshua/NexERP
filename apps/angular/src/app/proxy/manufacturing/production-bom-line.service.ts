import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateProductionBomLineDto, GetProductionBomLineListInput, ProductionBomLineDto } from './manufacturing.models';

@Injectable({
  providedIn: 'root',
})
export class ProductionBomLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateProductionBomLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionBomLineDto>({
      method: 'POST',
      url: '/api/erp/production-bom-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/production-bom-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionBomLineDto>({
      method: 'GET',
      url: `/api/erp/production-bom-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetProductionBomLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProductionBomLineDto>>({
      method: 'GET',
      url: '/api/erp/production-bom-line',
      params: { productionBomId: input.productionBomId, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateProductionBomLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductionBomLineDto>({
      method: 'PUT',
      url: `/api/erp/production-bom-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
