import type { CodeTableDto, CreateUpdateCodeTableDto, GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FaClassService {
  apiName = 'Erp';


  create = (input: CreateUpdateCodeTableDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CodeTableDto>({
      method: 'POST',
      url: '/api/erp/fa-class',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/fa-class/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CodeTableDto>({
      method: 'GET',
      url: `/api/erp/fa-class/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CodeTableDto>>({
      method: 'GET',
      url: '/api/erp/fa-class',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateCodeTableDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CodeTableDto>({
      method: 'PUT',
      url: `/api/erp/fa-class/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
