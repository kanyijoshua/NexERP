import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateStandardSalesLineDto, GetStandardSalesLineListInput, StandardSalesLineDto } from './standard-codes.models';

@Injectable({
  providedIn: 'root',
})
export class StandardSalesLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateStandardSalesLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardSalesLineDto>({
      method: 'POST',
      url: '/api/erp/standard-sales-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/standard-sales-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardSalesLineDto>({
      method: 'GET',
      url: `/api/erp/standard-sales-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetStandardSalesLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StandardSalesLineDto>>({
      method: 'GET',
      url: '/api/erp/standard-sales-line',
      params: { dynamicFilter: input.dynamicFilter, standardSalesCodeId: input.standardSalesCodeId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateStandardSalesLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardSalesLineDto>({
      method: 'PUT',
      url: `/api/erp/standard-sales-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
