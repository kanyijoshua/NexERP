import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateFaultCodeDto, FaultCodeDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class FaultCodeService {
  apiName = 'Erp';

  create = (input: CreateUpdateFaultCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FaultCodeDto>({
      method: 'POST',
      url: '/api/erp/fault-code',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/fault-code/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FaultCodeDto>({
      method: 'GET',
      url: `/api/erp/fault-code/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FaultCodeDto>>({
      method: 'GET',
      url: '/api/erp/fault-code',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateFaultCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FaultCodeDto>({
      method: 'PUT',
      url: `/api/erp/fault-code/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
