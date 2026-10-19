import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateItemJournalTemplateDto, ItemJournalTemplateDto } from './warehouse.models';
import type { GetCodeTableListInput } from '../companies/models';

@Injectable({
  providedIn: 'root',
})
export class ItemJournalTemplateService {
  apiName = 'Erp';

  create = (input: CreateUpdateItemJournalTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemJournalTemplateDto>({
      method: 'POST',
      url: '/api/erp/item-journal-template',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-journal-template/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemJournalTemplateDto>({
      method: 'GET',
      url: `/api/erp/item-journal-template/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemJournalTemplateDto>>({
      method: 'GET',
      url: '/api/erp/item-journal-template',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateItemJournalTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemJournalTemplateDto>({
      method: 'PUT',
      url: `/api/erp/item-journal-template/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
