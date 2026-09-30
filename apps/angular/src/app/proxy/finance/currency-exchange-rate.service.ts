import type { CreateUpdateCurrencyExchangeRateDto, CurrencyExchangeRateDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CurrencyExchangeRateService {
  apiName = 'Erp';


  create = (input: CreateUpdateCurrencyExchangeRateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CurrencyExchangeRateDto>({
      method: 'POST',
      url: '/api/erp/currency-exchange-rate',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/currency-exchange-rate/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CurrencyExchangeRateDto>({
      method: 'GET',
      url: `/api/erp/currency-exchange-rate/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CurrencyExchangeRateDto>>({
      method: 'GET',
      url: '/api/erp/currency-exchange-rate',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateCurrencyExchangeRateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CurrencyExchangeRateDto>({
      method: 'PUT',
      url: `/api/erp/currency-exchange-rate/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
