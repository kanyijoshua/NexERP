import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateResolutionCodeDto, ResolutionCodeDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class ResolutionCodeService {
  apiName = 'Erp';

  create = (input: CreateUpdateResolutionCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ResolutionCodeDto>({
      method: 'POST',
      url: '/api/erp/resolution-code',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/resolution-code/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ResolutionCodeDto>({
      method: 'GET',
      url: `/api/erp/resolution-code/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ResolutionCodeDto>>({
      method: 'GET',
      url: '/api/erp/resolution-code',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateResolutionCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ResolutionCodeDto>({
      method: 'PUT',
      url: `/api/erp/resolution-code/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
