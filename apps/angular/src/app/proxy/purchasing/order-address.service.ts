import type { CreateUpdateOrderAddressDto, GetOrderAddressListInput, OrderAddressDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class OrderAddressService {
  apiName = 'Erp';


  create = (input: CreateUpdateOrderAddressDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OrderAddressDto>({
      method: 'POST',
      url: '/api/erp/order-address',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/order-address/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OrderAddressDto>({
      method: 'GET',
      url: `/api/erp/order-address/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetOrderAddressListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<OrderAddressDto>>({
      method: 'GET',
      url: '/api/erp/order-address',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, vendorNo: input.vendorNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateOrderAddressDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OrderAddressDto>({
      method: 'PUT',
      url: `/api/erp/order-address/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
