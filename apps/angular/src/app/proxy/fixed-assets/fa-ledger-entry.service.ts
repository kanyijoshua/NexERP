import type { FALedgerEntryDto, GetFALedgerEntryListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FaLedgerEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FALedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/fa-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetFALedgerEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FALedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/fa-ledger-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, faNo: input.faNo, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
