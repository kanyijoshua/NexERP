import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CashFlowWorksheetLineDto, CreateUpdateCashFlowWorksheetLineDto, GetCashFlowLineListInput } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class CashFlowWorksheetLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateCashFlowWorksheetLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowWorksheetLineDto>({
      method: 'POST',
      url: '/api/erp/cash-flow-worksheet-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/cash-flow-worksheet-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowWorksheetLineDto>({
      method: 'GET',
      url: `/api/erp/cash-flow-worksheet-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCashFlowLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CashFlowWorksheetLineDto>>({
      method: 'GET',
      url: '/api/erp/cash-flow-worksheet-line',
      params: { cashFlowForecastNo: input.cashFlowForecastNo, dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCashFlowWorksheetLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowWorksheetLineDto>({
      method: 'PUT',
      url: `/api/erp/cash-flow-worksheet-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
