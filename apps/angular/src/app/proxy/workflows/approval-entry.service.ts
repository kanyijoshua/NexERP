import type { ApprovalCommentInput, ApprovalEntryDto, GetApprovalEntriesInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ApprovalEntryService {
  apiName = 'Erp';
  

  approve = (id: string, input: ApprovalCommentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/approval-entry/${id}/approve`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delegate = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/approval-entry/${id}/delegate`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetApprovalEntriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ApprovalEntryDto>>({
      method: 'GET',
      url: '/api/erp/approval-entry',
      params: { status: input.status, allStatuses: input.allStatuses, onlyMine: input.onlyMine, sentByMe: input.sentByMe, documentId: input.documentId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getMyOpenCount = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'GET',
      url: '/api/erp/approval-entry/my-open-count',
    },
    { apiName: this.apiName,...config });
  

  reject = (id: string, input: ApprovalCommentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/approval-entry/${id}/reject`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
