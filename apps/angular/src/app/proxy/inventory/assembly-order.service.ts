import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { AssemblyOrderDto, CreateUpdateAssemblyOrderDto, GetAssemblyOrderListInput } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class AssemblyOrderService {
  apiName = 'Erp';

  create = (input: CreateUpdateAssemblyOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyOrderDto>({
      method: 'POST',
      url: '/api/erp/assembly-order',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/assembly-order/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyOrderDto>({
      method: 'GET',
      url: `/api/erp/assembly-order/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetAssemblyOrderListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AssemblyOrderDto>>({
      method: 'GET',
      url: '/api/erp/assembly-order',
      params: { dynamicFilter: input.dynamicFilter, status: input.status, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  refreshLines = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyOrderDto>({
      method: 'POST',
      url: `/api/erp/assembly-order/${id}/refresh-lines`,
    },
    { apiName: this.apiName,...config });

  release = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyOrderDto>({
      method: 'POST',
      url: `/api/erp/assembly-order/${id}/release`,
    },
    { apiName: this.apiName,...config });

  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyOrderDto>({
      method: 'POST',
      url: `/api/erp/assembly-order/${id}/reopen`,
    },
    { apiName: this.apiName,...config });

  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyOrderDto>({
      method: 'POST',
      url: `/api/erp/assembly-order/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateAssemblyOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyOrderDto>({
      method: 'PUT',
      url: `/api/erp/assembly-order/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
