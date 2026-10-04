import type { ApprovalCommentLineDto, CreateUpdateApprovalCommentLineDto, GetApprovalCommentLineListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ApprovalCommentLineService {
  apiName = 'Erp';


  create = (input: CreateUpdateApprovalCommentLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalCommentLineDto>({
      method: 'POST',
      url: '/api/erp/approval-comment-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/approval-comment-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalCommentLineDto>({
      method: 'GET',
      url: `/api/erp/approval-comment-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetApprovalCommentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ApprovalCommentLineDto>>({
      method: 'GET',
      url: '/api/erp/approval-comment-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateApprovalCommentLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalCommentLineDto>({
      method: 'PUT',
      url: `/api/erp/approval-comment-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
