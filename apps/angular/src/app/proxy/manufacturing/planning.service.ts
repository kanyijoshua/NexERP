import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CarryOutResultDto, PagedManufacturingInput, PlanningLineDto, UpdatePlanningLineDto } from './manufacturing.models';

@Injectable({
  providedIn: 'root',
})
export class PlanningService {
  apiName = 'Erp';

  getList = (input: PagedManufacturingInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PlanningLineDto>>({
      method: 'GET',
      url: '/api/erp/planning',
      params: { dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PlanningLineDto>({
      method: 'GET',
      url: `/api/erp/planning/${id}`,
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: UpdatePlanningLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PlanningLineDto>({
      method: 'PUT',
      url: `/api/erp/planning/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/planning/${id}`,
    },
    { apiName: this.apiName,...config });

  calculate = (input: { toDate?: string }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: '/api/erp/planning/calculate',
      body: input,
    },
    { apiName: this.apiName,...config });

  carryOut = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, CarryOutResultDto>({
      method: 'POST',
      url: '/api/erp/planning/carry-out',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
