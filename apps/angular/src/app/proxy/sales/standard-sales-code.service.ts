import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateStandardSalesCodeDto, StandardSalesCodeDto } from './standard-codes.models';

@Injectable({
  providedIn: 'root',
})
export class StandardSalesCodeService {
  apiName = 'Erp';

  create = (input: CreateUpdateStandardSalesCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardSalesCodeDto>({
      method: 'POST',
      url: '/api/erp/standard-sales-code',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/standard-sales-code/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardSalesCodeDto>({
      method: 'GET',
      url: `/api/erp/standard-sales-code/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StandardSalesCodeDto>>({
      method: 'GET',
      url: '/api/erp/standard-sales-code',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateStandardSalesCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardSalesCodeDto>({
      method: 'PUT',
      url: `/api/erp/standard-sales-code/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
