import type { CreateGenJournalBatchDto, CreateGenJournalLineDto, GenJournalBatchDto, GenJournalLineDto, GenJournalPostingResultDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class general-journalService {
  apiName = 'Erp';
  

  createBatch = (input: CreateGenJournalBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GenJournalBatchDto>({
      method: 'POST',
      url: '/api/erp/general-journal/batch',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createLine = (input: CreateGenJournalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GenJournalLineDto>({
      method: 'POST',
      url: '/api/erp/general-journal/line',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  deleteLine = (lineId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/general-journal/line/${lineId}`,
    },
    { apiName: this.apiName,...config });
  

  getBatches = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<GenJournalBatchDto>>({
      method: 'GET',
      url: '/api/erp/general-journal/batches',
    },
    { apiName: this.apiName,...config });
  

  getLines = (batchId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<GenJournalLineDto>>({
      method: 'GET',
      url: `/api/erp/general-journal/lines/${batchId}`,
    },
    { apiName: this.apiName,...config });
  

  runPosting = (batchId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GenJournalPostingResultDto>({
      method: 'POST',
      url: `/api/erp/general-journal/run-posting/${batchId}`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
