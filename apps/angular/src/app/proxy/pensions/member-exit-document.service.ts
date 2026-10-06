import type { CreateUpdateMemberExitDocumentDto, GetMemberExitDocumentListInput, MemberExitDocumentDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MemberExitDocumentService {
  apiName = 'Erp';


  create = (input: CreateUpdateMemberExitDocumentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDocumentDto>({
      method: 'POST',
      url: '/api/erp/member-exit-document',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/member-exit-document/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDocumentDto>({
      method: 'GET',
      url: `/api/erp/member-exit-document/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetMemberExitDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MemberExitDocumentDto>>({
      method: 'GET',
      url: '/api/erp/member-exit-document',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, exitNo: input.exitNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateMemberExitDocumentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDocumentDto>({
      method: 'PUT',
      url: `/api/erp/member-exit-document/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
