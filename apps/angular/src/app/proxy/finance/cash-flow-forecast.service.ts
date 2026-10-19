import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { AnalysisMatrixInput, CashFlowByPeriodDto, CashFlowForecastDto, CreateUpdateCashFlowForecastDto, SuggestCashFlowInput } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class CashFlowForecastService {
  apiName = 'Erp';

  create = (input: CreateUpdateCashFlowForecastDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowForecastDto>({
      method: 'POST',
      url: '/api/erp/cash-flow-forecast',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/cash-flow-forecast/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowForecastDto>({
      method: 'GET',
      url: `/api/erp/cash-flow-forecast/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CashFlowForecastDto>>({
      method: 'GET',
      url: '/api/erp/cash-flow-forecast',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCashFlowForecastDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowForecastDto>({
      method: 'PUT',
      url: `/api/erp/cash-flow-forecast/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  suggestWorksheetLines = (id: string, input: SuggestCashFlowInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: `/api/erp/cash-flow-forecast/${id}/suggest-worksheet-lines`,
      body: input,
    },
    { apiName: this.apiName,...config });

  register = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: `/api/erp/cash-flow-forecast/${id}/register`,
    },
    { apiName: this.apiName,...config });

  getByPeriod = (id: string, input: AnalysisMatrixInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowByPeriodDto>({
      method: 'GET',
      url: `/api/erp/cash-flow-forecast/${id}/by-period`,
      params: { startDate: input.startDate, periodType: input.periodType, periodCount: input.periodCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
