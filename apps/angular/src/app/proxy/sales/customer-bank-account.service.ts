import type { CreateUpdateCustomerBankAccountDto, CustomerBankAccountDto, GetCustomerBankAccountListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CustomerBankAccountService {
  apiName = 'Erp';


  create = (input: CreateUpdateCustomerBankAccountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerBankAccountDto>({
      method: 'POST',
      url: '/api/erp/customer-bank-account',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/customer-bank-account/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerBankAccountDto>({
      method: 'GET',
      url: `/api/erp/customer-bank-account/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCustomerBankAccountListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CustomerBankAccountDto>>({
      method: 'GET',
      url: '/api/erp/customer-bank-account',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, customerNo: input.customerNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateCustomerBankAccountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerBankAccountDto>({
      method: 'PUT',
      url: `/api/erp/customer-bank-account/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
