import type { CreateUpdateGLAccountDto, GLAccountDto, GetGLAccountListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class gl-accountService {
  apiName = 'Erp';
  

  block = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/gl-account/${id}/block`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateGLAccountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLAccountDto>({
      method: 'POST',
      url: '/api/erp/gl-account',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/gl-account/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLAccountDto>({
      method: 'GET',
      url: `/api/erp/gl-account/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getByNo = (no: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLAccountDto>({
      method: 'GET',
      url: '/api/erp/gl-account/by-no',
      params: { no },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetGLAccountListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<GLAccountDto>>({
      method: 'GET',
      url: '/api/erp/gl-account',
      params: { filter: input.filter, accountCategory: input.accountCategory, accountType: input.accountType, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  unblock = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/gl-account/${id}/unblock`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateGLAccountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLAccountDto>({
      method: 'PUT',
      url: `/api/erp/gl-account/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
