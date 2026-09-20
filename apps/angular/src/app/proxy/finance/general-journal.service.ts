import type { CreateGenJournalBatchDto, CreateUpdateGenJournalLineDto, GenJournalBatchDto, GenJournalLineDto, GenJournalPostingResultDto, GetGenJournalBatchesInput, JournalCheckResultDto, PostingPreviewDto, UpdateGenJournalBatchDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class GeneralJournalService {
  apiName = 'Erp';
  

  check = (batchId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, JournalCheckResultDto>({
      method: 'POST',
      url: `/api/erp/general-journal/check/${batchId}`,
    },
    { apiName: this.apiName,...config });
  

  createBatch = (input: CreateGenJournalBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GenJournalBatchDto>({
      method: 'POST',
      url: '/api/erp/general-journal/batch',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createLine = (input: CreateUpdateGenJournalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GenJournalLineDto>({
      method: 'POST',
      url: '/api/erp/general-journal/line',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  deleteBatch = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/general-journal/${id}/batch`,
    },
    { apiName: this.apiName,...config });
  

  deleteLine = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/general-journal/${id}/line`,
    },
    { apiName: this.apiName,...config });
  

  getBatches = (input: GetGenJournalBatchesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<GenJournalBatchDto>>({
      method: 'GET',
      url: '/api/erp/general-journal/batches',
      params: { journalTemplateName: input.journalTemplateName },
    },
    { apiName: this.apiName,...config });
  

  getLines = (batchId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<GenJournalLineDto>>({
      method: 'GET',
      url: `/api/erp/general-journal/lines/${batchId}`,
    },
    { apiName: this.apiName,...config });
  

  preview = (batchId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostingPreviewDto>({
      method: 'POST',
      url: `/api/erp/general-journal/preview/${batchId}`,
    },
    { apiName: this.apiName,...config });
  

  runPosting = (batchId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GenJournalPostingResultDto>({
      method: 'POST',
      url: `/api/erp/general-journal/run-posting/${batchId}`,
    },
    { apiName: this.apiName,...config });
  

  updateBatch = (id: string, input: UpdateGenJournalBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GenJournalBatchDto>({
      method: 'PUT',
      url: `/api/erp/general-journal/${id}/batch`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateLine = (id: string, input: CreateUpdateGenJournalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GenJournalLineDto>({
      method: 'PUT',
      url: `/api/erp/general-journal/${id}/line`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
