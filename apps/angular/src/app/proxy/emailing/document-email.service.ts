import type { DocumentEmailPreviewDto, GetSentEmailLogListInput, PreviewDocumentEmailInput, SendDocumentEmailInput, SentEmailLogDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DocumentEmailService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SentEmailLogDto>({
      method: 'GET',
      url: `/api/erp/document-email/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetSentEmailLogListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SentEmailLogDto>>({
      method: 'GET',
      url: '/api/erp/document-email',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentType: input.documentType, documentId: input.documentId, documentNo: input.documentNo, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  preview = (input: PreviewDocumentEmailInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DocumentEmailPreviewDto>({
      method: 'POST',
      url: '/api/erp/document-email/preview',
      body: input,
    },
    { apiName: this.apiName,...config });


  send = (input: SendDocumentEmailInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SentEmailLogDto>({
      method: 'POST',
      url: '/api/erp/document-email/send',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
