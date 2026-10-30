import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CalculatePhysInvtLinesInput, CalculatePhysInvtLinesResultDto, CreateUpdatePhysInvtOrderDto, GetPhysInvtOrderListInput, PhysInvtOrderDto } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class PhysInvtOrderService {
  apiName = 'Erp';

  calculateLines = (id: string, input: CalculatePhysInvtLinesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CalculatePhysInvtLinesResultDto>({
      method: 'POST',
      url: `/api/erp/phys-invt-order/${id}/calculate-lines`,
      body: input,
    },
    { apiName: this.apiName,...config });

  create = (input: CreateUpdatePhysInvtOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PhysInvtOrderDto>({
      method: 'POST',
      url: '/api/erp/phys-invt-order',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/phys-invt-order/${id}`,
    },
    { apiName: this.apiName,...config });

  finish = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PhysInvtOrderDto>({
      method: 'POST',
      url: `/api/erp/phys-invt-order/${id}/finish`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PhysInvtOrderDto>({
      method: 'GET',
      url: `/api/erp/phys-invt-order/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPhysInvtOrderListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PhysInvtOrderDto>>({
      method: 'GET',
      url: '/api/erp/phys-invt-order',
      params: { dynamicFilter: input.dynamicFilter, status: input.status, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePhysInvtOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PhysInvtOrderDto>({
      method: 'PUT',
      url: `/api/erp/phys-invt-order/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
