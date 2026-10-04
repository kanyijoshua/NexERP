import type { BankAccountStatementLineDto, GetBankAccountStatementLineListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BankAccountStatementLineService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccountStatementLineDto>({
      method: 'GET',
      url: `/api/erp/bank-account-statement-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetBankAccountStatementLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BankAccountStatementLineDto>>({
      method: 'GET',
      url: '/api/erp/bank-account-statement-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, bankAccountNo: input.bankAccountNo, statementNo: input.statementNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
