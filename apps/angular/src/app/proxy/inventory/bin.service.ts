import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { BinContentDto, BinDto, CreateUpdateBinDto, GetBinContentListInput, GetBinListInput } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class BinService {
  apiName = 'Erp';

  create = (input: CreateUpdateBinDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BinDto>({
      method: 'POST',
      url: '/api/erp/bin',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/bin/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BinDto>({
      method: 'GET',
      url: `/api/erp/bin/${id}`,
    },
    { apiName: this.apiName,...config });

  getContents = (input: GetBinContentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BinContentDto>>({
      method: 'GET',
      url: '/api/erp/bin/contents',
      params: { locationCode: input.locationCode, binCode: input.binCode, itemNo: input.itemNo, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  getList = (input: GetBinListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BinDto>>({
      method: 'GET',
      url: '/api/erp/bin',
      params: { dynamicFilter: input.dynamicFilter, locationCode: input.locationCode, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateBinDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BinDto>({
      method: 'PUT',
      url: `/api/erp/bin/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
