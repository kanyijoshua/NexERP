import type { CreateUpdateGLBudgetEntryDto, GLBudgetEntryDto, GetGLBudgetEntryListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class GlBudgetEntryService {
  apiName = 'Erp';


  create = (input: CreateUpdateGLBudgetEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLBudgetEntryDto>({
      method: 'POST',
      url: '/api/erp/gl-budget-entry',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/gl-budget-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLBudgetEntryDto>({
      method: 'GET',
      url: `/api/erp/gl-budget-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetGLBudgetEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<GLBudgetEntryDto>>({
      method: 'GET',
      url: '/api/erp/gl-budget-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, budgetName: input.budgetName, glAccountNo: input.glAccountNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateGLBudgetEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLBudgetEntryDto>({
      method: 'PUT',
      url: `/api/erp/gl-budget-entry/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
