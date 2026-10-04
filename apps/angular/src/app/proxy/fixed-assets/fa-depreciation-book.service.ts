import type { CreateUpdateFADepreciationBookDto, FADepreciationBookDto, GetFADepreciationBookListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FaDepreciationBookService {
  apiName = 'Erp';


  create = (input: CreateUpdateFADepreciationBookDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FADepreciationBookDto>({
      method: 'POST',
      url: '/api/erp/fa-depreciation-book',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/fa-depreciation-book/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FADepreciationBookDto>({
      method: 'GET',
      url: `/api/erp/fa-depreciation-book/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetFADepreciationBookListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FADepreciationBookDto>>({
      method: 'GET',
      url: '/api/erp/fa-depreciation-book',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, faNo: input.faNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateFADepreciationBookDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FADepreciationBookDto>({
      method: 'PUT',
      url: `/api/erp/fa-depreciation-book/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
