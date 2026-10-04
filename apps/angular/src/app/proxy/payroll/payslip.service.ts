import type { GetPayslipListInput, PayslipDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PayslipService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PayslipDto>({
      method: 'GET',
      url: `/api/erp/payslip/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPayslipListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PayslipDto>>({
      method: 'GET',
      url: '/api/erp/payslip',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, payrollRunNo: input.payrollRunNo, employeeNo: input.employeeNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
