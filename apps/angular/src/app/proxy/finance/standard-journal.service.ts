import type { ApplyStandardJournalInput, GenJournalLineDto, GetStandardJournalsInput, SaveStandardJournalInput, StandardJournalDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StandardJournalService {
  apiName = 'Erp';
  

  applyToBatch = (input: ApplyStandardJournalInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<GenJournalLineDto>>({
      method: 'POST',
      url: '/api/erp/standard-journal/apply-to-batch',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/standard-journal/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStandardJournalsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<StandardJournalDto>>({
      method: 'GET',
      url: '/api/erp/standard-journal',
      params: { journalTemplateName: input.journalTemplateName },
    },
    { apiName: this.apiName,...config });
  

  saveFromBatch = (input: SaveStandardJournalInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardJournalDto>({
      method: 'POST',
      url: '/api/erp/standard-journal/save-from-batch',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
