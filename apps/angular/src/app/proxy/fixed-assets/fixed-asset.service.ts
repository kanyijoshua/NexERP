import type { CreateUpdateFixedAssetDto, FixedAssetDto, GetFixedAssetListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FixedAssetService {
  apiName = 'Erp';


  create = (input: CreateUpdateFixedAssetDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FixedAssetDto>({
      method: 'POST',
      url: '/api/erp/fixed-asset',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/fixed-asset/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FixedAssetDto>({
      method: 'GET',
      url: `/api/erp/fixed-asset/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetFixedAssetListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FixedAssetDto>>({
      method: 'GET',
      url: '/api/erp/fixed-asset',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateFixedAssetDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FixedAssetDto>({
      method: 'PUT',
      url: `/api/erp/fixed-asset/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
