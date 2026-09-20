import type { CreateUpdateGenJournalTemplateDto, GenJournalTemplateDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class JournalTemplateService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateGenJournalTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GenJournalTemplateDto>({
      method: 'POST',
      url: '/api/erp/journal-template',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/journal-template/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<GenJournalTemplateDto>>({
      method: 'GET',
      url: '/api/erp/journal-template',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateGenJournalTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GenJournalTemplateDto>({
      method: 'PUT',
      url: `/api/erp/journal-template/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
