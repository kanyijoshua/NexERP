import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateInteractionLogEntryDto, GetInteractionLogEntryListInput, InteractionLogEntryDto } from './crm.models';

@Injectable({
  providedIn: 'root',
})
export class InteractionLogEntryService {
  apiName = 'Erp';

  create = (input: CreateUpdateInteractionLogEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InteractionLogEntryDto>({
      method: 'POST',
      url: '/api/erp/interaction-log-entry',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/interaction-log-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InteractionLogEntryDto>({
      method: 'GET',
      url: `/api/erp/interaction-log-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetInteractionLogEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InteractionLogEntryDto>>({
      method: 'GET',
      url: '/api/erp/interaction-log-entry',
      params: { contactNo: input.contactNo, opportunityNo: input.opportunityNo, dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateInteractionLogEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InteractionLogEntryDto>({
      method: 'PUT',
      url: `/api/erp/interaction-log-entry/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
