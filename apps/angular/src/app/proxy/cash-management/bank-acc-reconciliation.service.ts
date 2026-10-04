import type { BankAccReconciliationDto, CreateUpdateBankAccReconciliationDto, GetBankAccReconciliationListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BankAccReconciliationService {
  apiName = 'Erp';


  create = (input: CreateUpdateBankAccReconciliationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccReconciliationDto>({
      method: 'POST',
      url: '/api/erp/bank-acc-reconciliation',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/bank-acc-reconciliation/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccReconciliationDto>({
      method: 'GET',
      url: `/api/erp/bank-acc-reconciliation/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetBankAccReconciliationListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BankAccReconciliationDto>>({
      method: 'GET',
      url: '/api/erp/bank-acc-reconciliation',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, bankAccountNo: input.bankAccountNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateBankAccReconciliationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccReconciliationDto>({
      method: 'PUT',
      url: `/api/erp/bank-acc-reconciliation/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
