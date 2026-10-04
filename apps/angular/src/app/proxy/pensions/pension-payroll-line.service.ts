import type { CreateUpdatePensionPayrollLineDto, GetPensionPayrollLineListInput, PensionPayrollLineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionPayrollLineService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionPayrollLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollLineDto>({
      method: 'POST',
      url: '/api/erp/pension-payroll-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-payroll-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollLineDto>({
      method: 'GET',
      url: `/api/erp/pension-payroll-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionPayrollLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionPayrollLineDto>>({
      method: 'GET',
      url: '/api/erp/pension-payroll-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, pensionerNo: input.pensionerNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionPayrollLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionPayrollLineDto>({
      method: 'PUT',
      url: `/api/erp/pension-payroll-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
