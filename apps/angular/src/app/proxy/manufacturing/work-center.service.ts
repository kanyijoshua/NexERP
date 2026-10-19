import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateWorkCenterDto, WorkCenterDto } from './manufacturing.models';

@Injectable({
  providedIn: 'root',
})
export class WorkCenterService {
  apiName = 'Erp';

  create = (input: CreateUpdateWorkCenterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkCenterDto>({
      method: 'POST',
      url: '/api/erp/work-center',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/work-center/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkCenterDto>({
      method: 'GET',
      url: `/api/erp/work-center/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WorkCenterDto>>({
      method: 'GET',
      url: '/api/erp/work-center',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateWorkCenterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkCenterDto>({
      method: 'PUT',
      url: `/api/erp/work-center/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
