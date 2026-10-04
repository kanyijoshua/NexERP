import type { CreateUpdateEmployeePayItemDto, EmployeePayItemDto, GetEmployeePayItemListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EmployeePayItemService {
  apiName = 'Erp';


  create = (input: CreateUpdateEmployeePayItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeePayItemDto>({
      method: 'POST',
      url: '/api/erp/employee-pay-item',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/employee-pay-item/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeePayItemDto>({
      method: 'GET',
      url: `/api/erp/employee-pay-item/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetEmployeePayItemListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<EmployeePayItemDto>>({
      method: 'GET',
      url: '/api/erp/employee-pay-item',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, employeeNo: input.employeeNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateEmployeePayItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EmployeePayItemDto>({
      method: 'PUT',
      url: `/api/erp/employee-pay-item/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
