import type { GetPartyLedgerEntryListInput, PartyLedgerEntryDto } from './periodic-activities.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CustomerLedgerEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PartyLedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/customer-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPartyLedgerEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PartyLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/customer-ledger-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, partyNo: input.partyNo, onlyOpen: input.onlyOpen, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
