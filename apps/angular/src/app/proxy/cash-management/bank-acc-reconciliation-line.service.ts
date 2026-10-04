import type { BankAccReconciliationLineDto, CreateUpdateBankAccReconciliationLineDto, GetBankAccReconciliationLineListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BankAccReconciliationLineService {
  apiName = 'Erp';


  create = (input: CreateUpdateBankAccReconciliationLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccReconciliationLineDto>({
      method: 'POST',
      url: '/api/erp/bank-acc-reconciliation-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/bank-acc-reconciliation-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccReconciliationLineDto>({
      method: 'GET',
      url: `/api/erp/bank-acc-reconciliation-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetBankAccReconciliationLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BankAccReconciliationLineDto>>({
      method: 'GET',
      url: '/api/erp/bank-acc-reconciliation-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, bankAccountNo: input.bankAccountNo, statementNo: input.statementNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateBankAccReconciliationLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccReconciliationLineDto>({
      method: 'PUT',
      url: `/api/erp/bank-acc-reconciliation-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
