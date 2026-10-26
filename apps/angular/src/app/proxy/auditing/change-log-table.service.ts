import type { ChangeLogTableDto, CreateUpdateChangeLogTableDto, GetChangeLogTableListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ChangeLogTableService {
  apiName = 'Erp';


  create = (input: CreateUpdateChangeLogTableDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ChangeLogTableDto>({
      method: 'POST',
      url: '/api/erp/change-log-table',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/change-log-table/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ChangeLogTableDto>({
      method: 'GET',
      url: `/api/erp/change-log-table/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetChangeLogTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ChangeLogTableDto>>({
      method: 'GET',
      url: '/api/erp/change-log-table',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateChangeLogTableDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ChangeLogTableDto>({
      method: 'PUT',
      url: `/api/erp/change-log-table/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
