import type { GetItemLedgerEntryListInput, ItemLedgerEntryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class item-ledger-entryService {
  apiName = 'Erp';
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemLedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/item-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetItemLedgerEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/item-ledger-entry',
      params: { itemId: input.itemId, documentNo: input.documentNo, entryType: input.entryType, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
