import type { CreateUpdateExitReasonDto, ExitReasonDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExitReasonService {
  apiName = 'Erp';


  create = (input: CreateUpdateExitReasonDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExitReasonDto>({
      method: 'POST',
      url: '/api/erp/exit-reason',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/exit-reason/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExitReasonDto>({
      method: 'GET',
      url: `/api/erp/exit-reason/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ExitReasonDto>>({
      method: 'GET',
      url: '/api/erp/exit-reason',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateExitReasonDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExitReasonDto>({
      method: 'PUT',
      url: `/api/erp/exit-reason/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
