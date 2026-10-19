import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateTimeSheetDto, GetTimeSheetListInput, TimeSheetDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class TimeSheetService {
  apiName = 'Erp';

  create = (input: CreateUpdateTimeSheetDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TimeSheetDto>({
      method: 'POST',
      url: '/api/erp/time-sheet',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/time-sheet/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TimeSheetDto>({
      method: 'GET',
      url: `/api/erp/time-sheet/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetTimeSheetListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<TimeSheetDto>>({
      method: 'GET',
      url: '/api/erp/time-sheet',
      params: { resourceNo: input.resourceNo, status: input.status, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateTimeSheetDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TimeSheetDto>({
      method: 'PUT',
      url: `/api/erp/time-sheet/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  submit = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TimeSheetDto>({
      method: 'POST',
      url: `/api/erp/time-sheet/${id}/submit`,
    },
    { apiName: this.apiName,...config });

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TimeSheetDto>({
      method: 'POST',
      url: `/api/erp/time-sheet/${id}/approve`,
    },
    { apiName: this.apiName,...config });

  reject = (id: string, input: { comment?: string }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TimeSheetDto>({
      method: 'POST',
      url: `/api/erp/time-sheet/${id}/reject`,
      body: input,
    },
    { apiName: this.apiName,...config });

  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, TimeSheetDto>({
      method: 'POST',
      url: `/api/erp/time-sheet/${id}/reopen`,
    },
    { apiName: this.apiName,...config });

  transferToJournal = (id: string, input: { projectJournalBatchId: string }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: `/api/erp/time-sheet/${id}/transfer-to-journal`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
