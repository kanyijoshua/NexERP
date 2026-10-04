import type { CreateUpdatePayrollDeductionDto, PayrollDeductionDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PayrollDeductionService {
  apiName = 'Erp';


  create = (input: CreateUpdatePayrollDeductionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollDeductionDto>({
      method: 'POST',
      url: '/api/erp/payroll-deduction',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/payroll-deduction/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollDeductionDto>({
      method: 'GET',
      url: `/api/erp/payroll-deduction/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PayrollDeductionDto>>({
      method: 'GET',
      url: '/api/erp/payroll-deduction',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePayrollDeductionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollDeductionDto>({
      method: 'PUT',
      url: `/api/erp/payroll-deduction/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
