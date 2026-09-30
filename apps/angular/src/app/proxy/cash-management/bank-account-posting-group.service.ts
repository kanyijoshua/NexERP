import type { BankAccountPostingGroupDto, CreateUpdateBankAccountPostingGroupDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BankAccountPostingGroupService {
  apiName = 'Erp';


  create = (input: CreateUpdateBankAccountPostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccountPostingGroupDto>({
      method: 'POST',
      url: '/api/erp/bank-account-posting-group',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/bank-account-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccountPostingGroupDto>({
      method: 'GET',
      url: `/api/erp/bank-account-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BankAccountPostingGroupDto>>({
      method: 'GET',
      url: '/api/erp/bank-account-posting-group',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateBankAccountPostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccountPostingGroupDto>({
      method: 'PUT',
      url: `/api/erp/bank-account-posting-group/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
