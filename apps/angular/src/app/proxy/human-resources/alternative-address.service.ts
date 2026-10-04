import type { AlternativeAddressDto, CreateUpdateAlternativeAddressDto, GetAlternativeAddressListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AlternativeAddressService {
  apiName = 'Erp';


  create = (input: CreateUpdateAlternativeAddressDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AlternativeAddressDto>({
      method: 'POST',
      url: '/api/erp/alternative-address',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/alternative-address/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AlternativeAddressDto>({
      method: 'GET',
      url: `/api/erp/alternative-address/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetAlternativeAddressListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AlternativeAddressDto>>({
      method: 'GET',
      url: '/api/erp/alternative-address',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, employeeNo: input.employeeNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateAlternativeAddressDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AlternativeAddressDto>({
      method: 'PUT',
      url: `/api/erp/alternative-address/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
