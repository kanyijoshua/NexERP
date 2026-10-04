import type { CreateUpdateEmployeeRelativeDto, EmployeeRelativeDto, GetEmployeeRelativeListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EmployeeRelativeService {
  apiName = 'Erp';


  create = (input: CreateUpdateEmployeeRelativeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeRelativeDto>({
      method: 'POST',
      url: '/api/erp/employee-relative',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/employee-relative/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeRelativeDto>({
      method: 'GET',
      url: `/api/erp/employee-relative/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetEmployeeRelativeListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<EmployeeRelativeDto>>({
      method: 'GET',
      url: '/api/erp/employee-relative',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, employeeNo: input.employeeNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateEmployeeRelativeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeRelativeDto>({
      method: 'PUT',
      url: `/api/erp/employee-relative/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
