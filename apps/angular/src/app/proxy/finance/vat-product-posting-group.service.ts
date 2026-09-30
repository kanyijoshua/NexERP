import type { CreateUpdatePostingGroupDto, PostingGroupDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class VatProductPostingGroupService {
  apiName = 'Erp';


  create = (input: CreateUpdatePostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostingGroupDto>({
      method: 'POST',
      url: '/api/erp/vat-product-posting-group',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/vat-product-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostingGroupDto>({
      method: 'GET',
      url: `/api/erp/vat-product-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PostingGroupDto>>({
      method: 'GET',
      url: '/api/erp/vat-product-posting-group',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostingGroupDto>({
      method: 'PUT',
      url: `/api/erp/vat-product-posting-group/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
