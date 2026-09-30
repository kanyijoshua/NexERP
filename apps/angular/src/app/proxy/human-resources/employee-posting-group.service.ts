import type { CreateUpdateEmployeePostingGroupDto, EmployeePostingGroupDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EmployeePostingGroupService {
  apiName = 'Erp';


  create = (input: CreateUpdateEmployeePostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeePostingGroupDto>({
      method: 'POST',
      url: '/api/erp/employee-posting-group',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/employee-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeePostingGroupDto>({
      method: 'GET',
      url: `/api/erp/employee-posting-group/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<EmployeePostingGroupDto>>({
      method: 'GET',
      url: '/api/erp/employee-posting-group',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateEmployeePostingGroupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeePostingGroupDto>({
      method: 'PUT',
      url: `/api/erp/employee-posting-group/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
