import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdatePriceListLineDto, GetPriceListLineListInput, PriceListLineDto } from './models';

@Injectable({
  providedIn: 'root',
})
export class PriceListLineService {
  apiName = 'Erp';

  create = (input: CreateUpdatePriceListLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PriceListLineDto>({
      method: 'POST',
      url: '/api/erp/price-list-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/price-list-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PriceListLineDto>({
      method: 'GET',
      url: `/api/erp/price-list-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPriceListLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PriceListLineDto>>({
      method: 'GET',
      url: '/api/erp/price-list-line',
      params: { dynamicFilter: input.dynamicFilter, priceListId: input.priceListId, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePriceListLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PriceListLineDto>({
      method: 'PUT',
      url: `/api/erp/price-list-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
