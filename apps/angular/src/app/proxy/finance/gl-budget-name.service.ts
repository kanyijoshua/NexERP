import type { CreateUpdateGLBudgetNameDto, GLBudgetNameDto, GetGLBudgetNameListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class GlBudgetNameService {
  apiName = 'Erp';


  create = (input: CreateUpdateGLBudgetNameDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLBudgetNameDto>({
      method: 'POST',
      url: '/api/erp/gl-budget-name',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/gl-budget-name/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLBudgetNameDto>({
      method: 'GET',
      url: `/api/erp/gl-budget-name/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetGLBudgetNameListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<GLBudgetNameDto>>({
      method: 'GET',
      url: '/api/erp/gl-budget-name',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateGLBudgetNameDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLBudgetNameDto>({
      method: 'PUT',
      url: `/api/erp/gl-budget-name/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
