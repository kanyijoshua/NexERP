import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateSubscriptionPlanDto, SubscriptionPlanDto } from './subscriptions.models';

@Injectable({
  providedIn: 'root',
})
export class SubscriptionPlanService {
  apiName = 'Erp';

  create = (input: CreateUpdateSubscriptionPlanDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SubscriptionPlanDto>({
      method: 'POST',
      url: '/api/erp/subscription-plan',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/subscription-plan/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SubscriptionPlanDto>({
      method: 'GET',
      url: `/api/erp/subscription-plan/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SubscriptionPlanDto>>({
      method: 'GET',
      url: '/api/erp/subscription-plan',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateSubscriptionPlanDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SubscriptionPlanDto>({
      method: 'PUT',
      url: `/api/erp/subscription-plan/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
