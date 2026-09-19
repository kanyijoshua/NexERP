import type { FinancialReportDto, FinancialReportPeriodInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class financial-reportService {
  apiName = 'Erp';
  

  getTrialBalance = (input: FinancialReportPeriodInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FinancialReportDto>({
      method: 'GET',
      url: '/api/erp/financial-report/trial-balance',
      params: { fromDate: input.fromDate, toDate: input.toDate },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
