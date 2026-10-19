import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateReminderLevelDto, GetReminderLevelListInput, ReminderLevelDto } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class ReminderLevelService {
  apiName = 'Erp';

  create = (input: CreateUpdateReminderLevelDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReminderLevelDto>({
      method: 'POST',
      url: '/api/erp/reminder-level',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/reminder-level/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReminderLevelDto>({
      method: 'GET',
      url: `/api/erp/reminder-level/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetReminderLevelListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ReminderLevelDto>>({
      method: 'GET',
      url: '/api/erp/reminder-level',
      params: { reminderTermsCode: input.reminderTermsCode, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateReminderLevelDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReminderLevelDto>({
      method: 'PUT',
      url: `/api/erp/reminder-level/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
