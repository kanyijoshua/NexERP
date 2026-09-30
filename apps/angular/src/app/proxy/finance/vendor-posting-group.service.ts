import type { CreateUpdateVendorPostingGroupDto, VendorPostingGroupDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class VendorPostingGroupService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateVendorPostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VendorPostingGroupDto>({
      method: 'POST',
      url: '/api/erp/vendor-posting-group',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/vendor-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VendorPostingGroupDto>({
      method: 'GET',
      url: `/api/erp/vendor-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<VendorPostingGroupDto>>({
      method: 'GET',
      url: '/api/erp/vendor-posting-group',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateVendorPostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VendorPostingGroupDto>({
      method: 'PUT',
      url: `/api/erp/vendor-posting-group/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
