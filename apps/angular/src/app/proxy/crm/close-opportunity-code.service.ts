import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CloseOpportunityCodeDto, CreateUpdateCloseOpportunityCodeDto } from './crm.models';

@Injectable({
  providedIn: 'root',
})
export class CloseOpportunityCodeService {
  apiName = 'Erp';

  create = (input: CreateUpdateCloseOpportunityCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CloseOpportunityCodeDto>({
      method: 'POST',
      url: '/api/erp/close-opportunity-code',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/close-opportunity-code/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CloseOpportunityCodeDto>({
      method: 'GET',
      url: `/api/erp/close-opportunity-code/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CloseOpportunityCodeDto>>({
      method: 'GET',
      url: '/api/erp/close-opportunity-code',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCloseOpportunityCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CloseOpportunityCodeDto>({
      method: 'PUT',
      url: `/api/erp/close-opportunity-code/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
