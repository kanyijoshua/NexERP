import type { CreateUpdateSemesterDto, SemesterDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SemesterService {
  apiName = 'Erp';


  create = (input: CreateUpdateSemesterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SemesterDto>({
      method: 'POST',
      url: '/api/erp/semester',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/semester/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SemesterDto>({
      method: 'GET',
      url: `/api/erp/semester/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SemesterDto>>({
      method: 'GET',
      url: '/api/erp/semester',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateSemesterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SemesterDto>({
      method: 'PUT',
      url: `/api/erp/semester/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
