import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateItemJournalLineDto, GetItemJournalLineListInput, ItemJournalLineDto } from './warehouse.models';

@Injectable({
  providedIn: 'root',
})
export class ItemJournalLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateItemJournalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemJournalLineDto>({
      method: 'POST',
      url: '/api/erp/item-journal-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-journal-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemJournalLineDto>({
      method: 'GET',
      url: `/api/erp/item-journal-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetItemJournalLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemJournalLineDto>>({
      method: 'GET',
      url: '/api/erp/item-journal-line',
      params: { dynamicFilter: input.dynamicFilter, itemJournalBatchId: input.itemJournalBatchId, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateItemJournalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemJournalLineDto>({
      method: 'PUT',
      url: `/api/erp/item-journal-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
