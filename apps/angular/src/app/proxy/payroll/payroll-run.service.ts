import type { CreateUpdatePayrollRunDto, GetPayrollRunListInput, PayrollRunDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PayrollRunService {
  apiName = 'Erp';


  create = (input: CreateUpdatePayrollRunDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollRunDto>({
      method: 'POST',
      url: '/api/erp/payroll-run',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/payroll-run/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollRunDto>({
      method: 'GET',
      url: `/api/erp/payroll-run/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPayrollRunListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PayrollRunDto>>({
      method: 'GET',
      url: '/api/erp/payroll-run',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePayrollRunDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollRunDto>({
      method: 'PUT',
      url: `/api/erp/payroll-run/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  calculate = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollRunDto>({
      method: 'POST',
      url: `/api/erp/payroll-run/${id}/calculate`,
    },
    { apiName: this.apiName,...config });

  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollRunDto>({
      method: 'POST',
      url: `/api/erp/payroll-run/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  raisePaymentVoucher = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollRunDto>({
      method: 'POST',
      url: `/api/erp/payroll-run/${id}/raise-payment-voucher`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
