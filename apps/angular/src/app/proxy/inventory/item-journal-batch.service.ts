import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CalculateInventoryInput, CreateUpdateItemJournalBatchDto, GetItemJournalBatchListInput, ItemJournalBatchDto, ItemJournalPostingResultDto } from './warehouse.models';

@Injectable({
  providedIn: 'root',
})
export class ItemJournalBatchService {
  apiName = 'Erp';

  create = (input: CreateUpdateItemJournalBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemJournalBatchDto>({
      method: 'POST',
      url: '/api/erp/item-journal-batch',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-journal-batch/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemJournalBatchDto>({
      method: 'GET',
      url: `/api/erp/item-journal-batch/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetItemJournalBatchListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemJournalBatchDto>>({
      method: 'GET',
      url: '/api/erp/item-journal-batch',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, templateType: input.templateType, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateItemJournalBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemJournalBatchDto>({
      method: 'PUT',
      url: `/api/erp/item-journal-batch/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemJournalPostingResultDto>({
      method: 'POST',
      url: `/api/erp/item-journal-batch/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  calculateInventory = (id: string, input: CalculateInventoryInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: `/api/erp/item-journal-batch/${id}/calculate-inventory`,
      body: input,
    },
    { apiName: this.apiName,...config });

  calculateInventoryValue = (id: string, input: CalculateInventoryInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: `/api/erp/item-journal-batch/${id}/calculate-inventory-value`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
