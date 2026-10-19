import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateReminderTermsDto, ReminderTermsDto } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class ReminderTermsService {
  apiName = 'Erp';

  create = (input: CreateUpdateReminderTermsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReminderTermsDto>({
      method: 'POST',
      url: '/api/erp/reminder-terms',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/reminder-terms/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReminderTermsDto>({
      method: 'GET',
      url: `/api/erp/reminder-terms/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ReminderTermsDto>>({
      method: 'GET',
      url: '/api/erp/reminder-terms',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateReminderTermsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReminderTermsDto>({
      method: 'PUT',
      url: `/api/erp/reminder-terms/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
