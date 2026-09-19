import type { ApprovalEntryDto, GetApprovalEntriesInput, WorkflowDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class workflowService {
  apiName = 'Erp';
  

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/workflow/${id}/approve`,
    },
    { apiName: this.apiName,...config });
  

  disable = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/workflow/${id}/disable`,
    },
    { apiName: this.apiName,...config });
  

  enable = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/workflow/${id}/enable`,
    },
    { apiName: this.apiName,...config });
  

  getApprovalEntries = (input: GetApprovalEntriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ApprovalEntryDto>>({
      method: 'GET',
      url: '/api/erp/workflow/approval-entries',
      params: { status: input.status, onlyMine: input.onlyMine, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<WorkflowDto>>({
      method: 'GET',
      url: '/api/erp/workflow',
    },
    { apiName: this.apiName,...config });
  

  reject = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/workflow/${id}/reject`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
