import type { CreateUpdateItemVendorDto, GetItemVendorListInput, ItemVendorDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ItemVendorService {
  apiName = 'Erp';


  create = (input: CreateUpdateItemVendorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemVendorDto>({
      method: 'POST',
      url: '/api/erp/item-vendor',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-vendor/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemVendorDto>({
      method: 'GET',
      url: `/api/erp/item-vendor/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetItemVendorListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemVendorDto>>({
      method: 'GET',
      url: '/api/erp/item-vendor',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, vendorNo: input.vendorNo, itemNo: input.itemNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateItemVendorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemVendorDto>({
      method: 'PUT',
      url: `/api/erp/item-vendor/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
