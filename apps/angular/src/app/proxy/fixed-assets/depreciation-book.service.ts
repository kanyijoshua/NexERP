import type { CreateUpdateDepreciationBookDto, DepreciationBookDto } from './base-tables.models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DepreciationBookService {
  apiName = 'Erp';


  create = (input: CreateUpdateDepreciationBookDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DepreciationBookDto>({
      method: 'POST',
      url: '/api/erp/depreciation-book',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/depreciation-book/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DepreciationBookDto>({
      method: 'GET',
      url: `/api/erp/depreciation-book/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<DepreciationBookDto>>({
      method: 'GET',
      url: '/api/erp/depreciation-book',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateDepreciationBookDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DepreciationBookDto>({
      method: 'PUT',
      url: `/api/erp/depreciation-book/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
