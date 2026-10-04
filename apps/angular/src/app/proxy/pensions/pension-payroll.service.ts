import type { CreateUpdatePensionPayrollHeaderDto, GetPensionPayrollListInput, PensionPayrollHeaderDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionPayrollService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionPayrollHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollHeaderDto>({
      method: 'POST',
      url: '/api/erp/pension-payroll',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-payroll/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollHeaderDto>({
      method: 'GET',
      url: `/api/erp/pension-payroll/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionPayrollListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionPayrollHeaderDto>>({
      method: 'GET',
      url: '/api/erp/pension-payroll',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, schemeCode: input.schemeCode, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionPayrollHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollHeaderDto>({
      method: 'PUT',
      url: `/api/erp/pension-payroll/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  suggestLines = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollHeaderDto>({
      method: 'POST',
      url: `/api/erp/pension-payroll/${id}/suggest-lines`,
    },
    { apiName: this.apiName,...config });


  release = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollHeaderDto>({
      method: 'POST',
      url: `/api/erp/pension-payroll/${id}/release`,
    },
    { apiName: this.apiName,...config });


  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollHeaderDto>({
      method: 'POST',
      url: `/api/erp/pension-payroll/${id}/reopen`,
    },
    { apiName: this.apiName,...config });


  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollHeaderDto>({
      method: 'POST',
      url: `/api/erp/pension-payroll/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });


  raisePaymentVoucher = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollHeaderDto>({
      method: 'POST',
      url: `/api/erp/pension-payroll/${id}/raise-payment-voucher`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
