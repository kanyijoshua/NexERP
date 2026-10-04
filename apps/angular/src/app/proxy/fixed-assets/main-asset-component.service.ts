import type { CreateUpdateMainAssetComponentDto, GetMainAssetComponentListInput, MainAssetComponentDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MainAssetComponentService {
  apiName = 'Erp';


  create = (input: CreateUpdateMainAssetComponentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MainAssetComponentDto>({
      method: 'POST',
      url: '/api/erp/main-asset-component',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/main-asset-component/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MainAssetComponentDto>({
      method: 'GET',
      url: `/api/erp/main-asset-component/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetMainAssetComponentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MainAssetComponentDto>>({
      method: 'GET',
      url: '/api/erp/main-asset-component',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, mainAssetNo: input.mainAssetNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateMainAssetComponentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MainAssetComponentDto>({
      method: 'PUT',
      url: `/api/erp/main-asset-component/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
