import type { GetPayslipListInput, PayslipLineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PayslipLineService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayslipLineDto>({
      method: 'GET',
      url: `/api/erp/payslip-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPayslipListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PayslipLineDto>>({
      method: 'GET',
      url: '/api/erp/payslip-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, payrollRunNo: input.payrollRunNo, employeeNo: input.employeeNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
