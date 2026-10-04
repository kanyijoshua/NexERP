import type { CreateUpdateLumpsumTaxTableDto, LumpsumTaxTableDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LumpsumTaxTableService {
  apiName = 'Erp';


  create = (input: CreateUpdateLumpsumTaxTableDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LumpsumTaxTableDto>({
      method: 'POST',
      url: '/api/erp/lumpsum-tax-table',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/lumpsum-tax-table/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LumpsumTaxTableDto>({
      method: 'GET',
      url: `/api/erp/lumpsum-tax-table/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LumpsumTaxTableDto>>({
      method: 'GET',
      url: '/api/erp/lumpsum-tax-table',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateLumpsumTaxTableDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LumpsumTaxTableDto>({
      method: 'PUT',
      url: `/api/erp/lumpsum-tax-table/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
