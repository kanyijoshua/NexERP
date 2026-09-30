import type { CreateUpdateCustomerPostingGroupDto, CustomerPostingGroupDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CustomerPostingGroupService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateCustomerPostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerPostingGroupDto>({
      method: 'POST',
      url: '/api/erp/customer-posting-group',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/customer-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerPostingGroupDto>({
      method: 'GET',
      url: `/api/erp/customer-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CustomerPostingGroupDto>>({
      method: 'GET',
      url: '/api/erp/customer-posting-group',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateCustomerPostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerPostingGroupDto>({
      method: 'PUT',
      url: `/api/erp/customer-posting-group/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
