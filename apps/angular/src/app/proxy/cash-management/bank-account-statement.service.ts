import type { BankAccountStatementDto, GetBankAccountStatementListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BankAccountStatementService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccountStatementDto>({
      method: 'GET',
      url: `/api/erp/bank-account-statement/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetBankAccountStatementListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BankAccountStatementDto>>({
      method: 'GET',
      url: '/api/erp/bank-account-statement',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, bankAccountNo: input.bankAccountNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
