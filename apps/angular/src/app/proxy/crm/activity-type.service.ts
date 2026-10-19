import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { ActivityTypeDto, CreateUpdateActivityTypeDto } from './crm.models';

@Injectable({
  providedIn: 'root',
})
export class ActivityTypeService {
  apiName = 'Erp';

  create = (input: CreateUpdateActivityTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ActivityTypeDto>({
      method: 'POST',
      url: '/api/erp/activity-type',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/activity-type/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ActivityTypeDto>({
      method: 'GET',
      url: `/api/erp/activity-type/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ActivityTypeDto>>({
      method: 'GET',
      url: '/api/erp/activity-type',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateActivityTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ActivityTypeDto>({
      method: 'PUT',
      url: `/api/erp/activity-type/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
