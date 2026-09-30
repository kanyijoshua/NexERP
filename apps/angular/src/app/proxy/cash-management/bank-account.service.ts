import type { BankAccountDto, CreateUpdateBankAccountDto, GetBankAccountListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BankAccountService {
  apiName = 'Erp';


  create = (input: CreateUpdateBankAccountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccountDto>({
      method: 'POST',
      url: '/api/erp/bank-account',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/bank-account/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccountDto>({
      method: 'GET',
      url: `/api/erp/bank-account/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetBankAccountListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BankAccountDto>>({
      method: 'GET',
      url: '/api/erp/bank-account',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateBankAccountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccountDto>({
      method: 'PUT',
      url: `/api/erp/bank-account/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  block = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/bank-account/${id}/block`,
    },
    { apiName: this.apiName,...config });


  unblock = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/bank-account/${id}/unblock`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
