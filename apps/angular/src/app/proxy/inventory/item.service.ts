import type { CreateUpdateItemDto, GetItemListInput, ItemDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ItemService {
  apiName = 'Erp';
  

  block = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/item/${id}/block`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemDto>({
      method: 'POST',
      url: '/api/erp/item',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemDto>({
      method: 'GET',
      url: `/api/erp/item/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getByNo = (no: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemDto>({
      method: 'GET',
      url: '/api/erp/item/by-no',
      params: { no },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetItemListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemDto>>({
      method: 'GET',
      url: '/api/erp/item',
      params: { filter: input.filter, type: input.type, itemCategoryId: input.itemCategoryId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  unblock = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/item/${id}/unblock`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemDto>({
      method: 'PUT',
      url: `/api/erp/item/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
