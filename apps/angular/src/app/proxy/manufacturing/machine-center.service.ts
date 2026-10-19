import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateMachineCenterDto, MachineCenterDto } from './manufacturing.models';

@Injectable({
  providedIn: 'root',
})
export class MachineCenterService {
  apiName = 'Erp';

  create = (input: CreateUpdateMachineCenterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MachineCenterDto>({
      method: 'POST',
      url: '/api/erp/machine-center',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/machine-center/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MachineCenterDto>({
      method: 'GET',
      url: `/api/erp/machine-center/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MachineCenterDto>>({
      method: 'GET',
      url: '/api/erp/machine-center',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateMachineCenterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MachineCenterDto>({
      method: 'PUT',
      url: `/api/erp/machine-center/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
