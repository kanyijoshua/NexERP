import type { CreateUpdateCustomerDto, CustomerDto, GetCustomerListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  apiName = 'Erp';
  

  block = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/customer/${id}/block`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateCustomerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDto>({
      method: 'POST',
      url: '/api/erp/customer',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/customer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDto>({
      method: 'GET',
      url: `/api/erp/customer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getByNo = (no: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDto>({
      method: 'GET',
      url: '/api/erp/customer/by-no',
      params: { no },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetCustomerListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CustomerDto>>({
      method: 'GET',
      url: '/api/erp/customer',
      params: { filter: input.filter, blocked: input.blocked, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  unblock = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/customer/${id}/unblock`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateCustomerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDto>({
      method: 'PUT',
      url: `/api/erp/customer/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
