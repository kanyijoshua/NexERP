import type { CreateUpdateItemCategoryDto, ItemCategoryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ItemCategoryService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateItemCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemCategoryDto>({
      method: 'POST',
      url: '/api/erp/item-category',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-category/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemCategoryDto>({
      method: 'GET',
      url: `/api/erp/item-category/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemCategoryDto>>({
      method: 'GET',
      url: '/api/erp/item-category',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateItemCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemCategoryDto>({
      method: 'PUT',
      url: `/api/erp/item-category/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
