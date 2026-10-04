import type { CreateUpdateFASubclassDto, FASubclassDto } from './base-tables.models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FaSubclassService {
  apiName = 'Erp';


  create = (input: CreateUpdateFASubclassDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FASubclassDto>({
      method: 'POST',
      url: '/api/erp/fa-subclass',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/fa-subclass/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FASubclassDto>({
      method: 'GET',
      url: `/api/erp/fa-subclass/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FASubclassDto>>({
      method: 'GET',
      url: '/api/erp/fa-subclass',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateFASubclassDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FASubclassDto>({
      method: 'PUT',
      url: `/api/erp/fa-subclass/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
