import type { CreateUpdateExitReasonDocumentDto, ExitReasonDocumentDto, GetExitReasonDocumentListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExitReasonDocumentService {
  apiName = 'Erp';


  create = (input: CreateUpdateExitReasonDocumentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExitReasonDocumentDto>({
      method: 'POST',
      url: '/api/erp/exit-reason-document',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/exit-reason-document/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExitReasonDocumentDto>({
      method: 'GET',
      url: `/api/erp/exit-reason-document/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetExitReasonDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ExitReasonDocumentDto>>({
      method: 'GET',
      url: '/api/erp/exit-reason-document',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, exitReasonCode: input.exitReasonCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateExitReasonDocumentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExitReasonDocumentDto>({
      method: 'PUT',
      url: `/api/erp/exit-reason-document/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
