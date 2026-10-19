import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CalculateDepreciationInput, CreateUpdateFAJournalBatchDto, FAJournalBatchDto, FAJournalPostingResultDto } from './fixed-asset-posting.models';

@Injectable({
  providedIn: 'root',
})
export class FAJournalBatchService {
  apiName = 'Erp';

  create = (input: CreateUpdateFAJournalBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FAJournalBatchDto>({
      method: 'POST',
      url: '/api/erp/fa-journal-batch',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/fa-journal-batch/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FAJournalBatchDto>({
      method: 'GET',
      url: `/api/erp/fa-journal-batch/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FAJournalBatchDto>>({
      method: 'GET',
      url: '/api/erp/fa-journal-batch',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateFAJournalBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FAJournalBatchDto>({
      method: 'PUT',
      url: `/api/erp/fa-journal-batch/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FAJournalPostingResultDto>({
      method: 'POST',
      url: `/api/erp/fa-journal-batch/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  calculateDepreciation = (id: string, input: CalculateDepreciationInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: `/api/erp/fa-journal-batch/${id}/calculate-depreciation`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
