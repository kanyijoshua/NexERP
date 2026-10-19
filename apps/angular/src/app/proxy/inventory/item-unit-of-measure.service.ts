import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateItemUnitOfMeasureDto, GetItemChildListInput, ItemUnitOfMeasureDto } from './warehouse.models';

@Injectable({
  providedIn: 'root',
})
export class ItemUnitOfMeasureService {
  apiName = 'Erp';

  create = (input: CreateUpdateItemUnitOfMeasureDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemUnitOfMeasureDto>({
      method: 'POST',
      url: '/api/erp/item-unit-of-measure',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-unit-of-measure/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemUnitOfMeasureDto>({
      method: 'GET',
      url: `/api/erp/item-unit-of-measure/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetItemChildListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemUnitOfMeasureDto>>({
      method: 'GET',
      url: '/api/erp/item-unit-of-measure',
      params: { dynamicFilter: input.dynamicFilter, itemNo: input.itemNo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateItemUnitOfMeasureDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemUnitOfMeasureDto>({
      method: 'PUT',
      url: `/api/erp/item-unit-of-measure/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
