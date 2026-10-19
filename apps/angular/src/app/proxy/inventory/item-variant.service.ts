import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateItemVariantDto, GetItemChildListInput, ItemVariantDto } from './warehouse.models';

@Injectable({
  providedIn: 'root',
})
export class ItemVariantService {
  apiName = 'Erp';

  create = (input: CreateUpdateItemVariantDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemVariantDto>({
      method: 'POST',
      url: '/api/erp/item-variant',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-variant/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemVariantDto>({
      method: 'GET',
      url: `/api/erp/item-variant/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetItemChildListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemVariantDto>>({
      method: 'GET',
      url: '/api/erp/item-variant',
      params: { dynamicFilter: input.dynamicFilter, itemNo: input.itemNo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateItemVariantDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemVariantDto>({
      method: 'PUT',
      url: `/api/erp/item-variant/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
