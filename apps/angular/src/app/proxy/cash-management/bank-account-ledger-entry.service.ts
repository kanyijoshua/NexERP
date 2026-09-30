import type { BankAccountLedgerEntryDto, GetBankAccountLedgerEntryListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BankAccountLedgerEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankAccountLedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/bank-account-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetBankAccountLedgerEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BankAccountLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/bank-account-ledger-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, bankAccountId: input.bankAccountId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
