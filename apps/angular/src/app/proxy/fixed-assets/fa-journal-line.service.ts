import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateFAJournalLineDto, FAJournalLineDto, GetFAJournalLineListInput } from './fixed-asset-posting.models';

@Injectable({
  providedIn: 'root',
})
export class FAJournalLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateFAJournalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FAJournalLineDto>({
      method: 'POST',
      url: '/api/erp/fa-journal-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/fa-journal-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FAJournalLineDto>({
      method: 'GET',
      url: `/api/erp/fa-journal-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetFAJournalLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FAJournalLineDto>>({
      method: 'GET',
      url: '/api/erp/fa-journal-line',
      params: { faJournalBatchId: input.faJournalBatchId, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateFAJournalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FAJournalLineDto>({
      method: 'PUT',
      url: `/api/erp/fa-journal-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
