import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CashFlowForecastEntryDto, GetCashFlowLineListInput } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class CashFlowForecastEntryService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowForecastEntryDto>({
      method: 'GET',
      url: `/api/erp/cash-flow-forecast-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCashFlowLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CashFlowForecastEntryDto>>({
      method: 'GET',
      url: '/api/erp/cash-flow-forecast-entry',
      params: { cashFlowForecastNo: input.cashFlowForecastNo, dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
