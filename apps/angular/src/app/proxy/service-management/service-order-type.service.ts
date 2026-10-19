import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateServiceOrderTypeDto, ServiceOrderTypeDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class ServiceOrderTypeService {
  apiName = 'Erp';

  create = (input: CreateUpdateServiceOrderTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceOrderTypeDto>({
      method: 'POST',
      url: '/api/erp/service-order-type',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/service-order-type/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceOrderTypeDto>({
      method: 'GET',
      url: `/api/erp/service-order-type/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ServiceOrderTypeDto>>({
      method: 'GET',
      url: '/api/erp/service-order-type',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateServiceOrderTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceOrderTypeDto>({
      method: 'PUT',
      url: `/api/erp/service-order-type/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
