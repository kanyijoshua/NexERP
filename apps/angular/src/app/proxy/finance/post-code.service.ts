import type { CreateUpdatePostCodeDto, GetPostCodeListInput, PostCodeDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PostCodeService {
  apiName = 'Erp';


  create = (input: CreateUpdatePostCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostCodeDto>({
      method: 'POST',
      url: '/api/erp/post-code',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/post-code/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostCodeDto>({
      method: 'GET',
      url: `/api/erp/post-code/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPostCodeListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PostCodeDto>>({
      method: 'GET',
      url: '/api/erp/post-code',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePostCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostCodeDto>({
      method: 'PUT',
      url: `/api/erp/post-code/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
