import type { CreateUpdateCurrencyDto, CurrencyDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CurrencyService {
  apiName = 'Erp';


  create = (input: CreateUpdateCurrencyDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CurrencyDto>({
      method: 'POST',
      url: '/api/erp/currency',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/currency/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CurrencyDto>({
      method: 'GET',
      url: `/api/erp/currency/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CurrencyDto>>({
      method: 'GET',
      url: '/api/erp/currency',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateCurrencyDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CurrencyDto>({
      method: 'PUT',
      url: `/api/erp/currency/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
