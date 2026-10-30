import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateICPartnerDto, ICPartnerDto } from './intercompany.models';

@Injectable({
  providedIn: 'root',
})
export class ICPartnerService {
  apiName = 'Erp';

  create = (input: CreateUpdateICPartnerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ICPartnerDto>({
      method: 'POST',
      url: '/api/erp/ic-partner',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/ic-partner/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ICPartnerDto>({
      method: 'GET',
      url: `/api/erp/ic-partner/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ICPartnerDto>>({
      method: 'GET',
      url: '/api/erp/ic-partner',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateICPartnerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ICPartnerDto>({
      method: 'PUT',
      url: `/api/erp/ic-partner/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
