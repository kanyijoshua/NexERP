import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateServiceContractLineDto, GetServiceContractLineListInput, ServiceContractLineDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class ServiceContractLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateServiceContractLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceContractLineDto>({
      method: 'POST',
      url: '/api/erp/service-contract-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/service-contract-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceContractLineDto>({
      method: 'GET',
      url: `/api/erp/service-contract-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetServiceContractLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ServiceContractLineDto>>({
      method: 'GET',
      url: '/api/erp/service-contract-line',
      params: { serviceContractId: input.serviceContractId, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateServiceContractLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceContractLineDto>({
      method: 'PUT',
      url: `/api/erp/service-contract-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
