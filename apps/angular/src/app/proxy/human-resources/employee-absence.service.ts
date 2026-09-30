import type { CreateUpdateEmployeeAbsenceDto, EmployeeAbsenceDto, GetEmployeeAbsenceListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EmployeeAbsenceService {
  apiName = 'Erp';


  create = (input: CreateUpdateEmployeeAbsenceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeAbsenceDto>({
      method: 'POST',
      url: '/api/erp/employee-absence',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/employee-absence/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeAbsenceDto>({
      method: 'GET',
      url: `/api/erp/employee-absence/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetEmployeeAbsenceListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<EmployeeAbsenceDto>>({
      method: 'GET',
      url: '/api/erp/employee-absence',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, employeeId: input.employeeId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateEmployeeAbsenceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeeAbsenceDto>({
      method: 'PUT',
      url: `/api/erp/employee-absence/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
