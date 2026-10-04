import type { CreateUpdateEmployeeQualificationDto, EmployeeQualificationDto, GetEmployeeQualificationListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EmployeeQualificationService {
  apiName = 'Erp';


  create = (input: CreateUpdateEmployeeQualificationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeQualificationDto>({
      method: 'POST',
      url: '/api/erp/employee-qualification',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/employee-qualification/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeQualificationDto>({
      method: 'GET',
      url: `/api/erp/employee-qualification/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetEmployeeQualificationListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<EmployeeQualificationDto>>({
      method: 'GET',
      url: '/api/erp/employee-qualification',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, employeeNo: input.employeeNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateEmployeeQualificationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeQualificationDto>({
      method: 'PUT',
      url: `/api/erp/employee-qualification/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
