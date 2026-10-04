import type { CreateUpdatePayrollEarningDto, PayrollEarningDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PayrollEarningService {
  apiName = 'Erp';


  create = (input: CreateUpdatePayrollEarningDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollEarningDto>({
      method: 'POST',
      url: '/api/erp/payroll-earning',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/payroll-earning/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollEarningDto>({
      method: 'GET',
      url: `/api/erp/payroll-earning/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PayrollEarningDto>>({
      method: 'GET',
      url: '/api/erp/payroll-earning',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePayrollEarningDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollEarningDto>({
      method: 'PUT',
      url: `/api/erp/payroll-earning/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
