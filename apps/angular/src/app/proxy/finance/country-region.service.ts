import type { CountryRegionDto, CreateUpdateCountryRegionDto } from './base-tables.models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CountryRegionService {
  apiName = 'Erp';


  create = (input: CreateUpdateCountryRegionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CountryRegionDto>({
      method: 'POST',
      url: '/api/erp/country-region',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/country-region/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CountryRegionDto>({
      method: 'GET',
      url: `/api/erp/country-region/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CountryRegionDto>>({
      method: 'GET',
      url: '/api/erp/country-region',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateCountryRegionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CountryRegionDto>({
      method: 'PUT',
      url: `/api/erp/country-region/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
