import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateCustomerPriceGroupDto, CustomerPriceGroupDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';

@Injectable({
  providedIn: 'root',
})
export class CustomerPriceGroupService {
  apiName = 'Erp';

  create = (input: CreateUpdateCustomerPriceGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerPriceGroupDto>({
      method: 'POST',
      url: '/api/erp/customer-price-group',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/customer-price-group/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerPriceGroupDto>({
      method: 'GET',
      url: `/api/erp/customer-price-group/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CustomerPriceGroupDto>>({
      method: 'GET',
      url: '/api/erp/customer-price-group',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCustomerPriceGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerPriceGroupDto>({
      method: 'PUT',
      url: `/api/erp/customer-price-group/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
