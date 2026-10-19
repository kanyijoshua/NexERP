import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CashFlowManualEntryDto, CreateUpdateCashFlowManualEntryDto } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class CashFlowManualEntryService {
  apiName = 'Erp';

  create = (input: CreateUpdateCashFlowManualEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowManualEntryDto>({
      method: 'POST',
      url: '/api/erp/cash-flow-manual-entry',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/cash-flow-manual-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowManualEntryDto>({
      method: 'GET',
      url: `/api/erp/cash-flow-manual-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CashFlowManualEntryDto>>({
      method: 'GET',
      url: '/api/erp/cash-flow-manual-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCashFlowManualEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CashFlowManualEntryDto>({
      method: 'PUT',
      url: `/api/erp/cash-flow-manual-entry/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
