import type { CreateUpdatePayrollTaxBandDto, GetPayrollTaxBandListInput, PayrollTaxBandDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PayrollTaxBandService {
  apiName = 'Erp';


  create = (input: CreateUpdatePayrollTaxBandDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollTaxBandDto>({
      method: 'POST',
      url: '/api/erp/payroll-tax-band',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/payroll-tax-band/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollTaxBandDto>({
      method: 'GET',
      url: `/api/erp/payroll-tax-band/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPayrollTaxBandListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PayrollTaxBandDto>>({
      method: 'GET',
      url: '/api/erp/payroll-tax-band',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePayrollTaxBandDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayrollTaxBandDto>({
      method: 'PUT',
      url: `/api/erp/payroll-tax-band/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
