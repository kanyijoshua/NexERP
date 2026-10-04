import type { DocumentAttachmentDto, GetDocumentAttachmentListInput, UpdateDocumentAttachmentDto, UploadDocumentAttachmentInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DocumentAttachmentService {
  apiName = 'Erp';


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/document-attachment/${id}`,
    },
    { apiName: this.apiName,...config });


  download = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'GET',
      responseType: 'blob',
      url: `/api/erp/document-attachment/${id}/download`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetDocumentAttachmentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<DocumentAttachmentDto>>({
      method: 'GET',
      url: '/api/erp/document-attachment',
      params: { entityName: input.entityName, recordId: input.recordId },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: UpdateDocumentAttachmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DocumentAttachmentDto>({
      method: 'PUT',
      url: `/api/erp/document-attachment/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  upload = (input: UploadDocumentAttachmentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DocumentAttachmentDto>({
      method: 'POST',
      url: '/api/erp/document-attachment/upload',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
