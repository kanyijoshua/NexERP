import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdatePhysInvtOrderLineDto, GetPhysInvtOrderLineListInput, PhysInvtOrderLineDto } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class PhysInvtOrderLineService {
  apiName = 'Erp';

  create = (input: CreateUpdatePhysInvtOrderLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PhysInvtOrderLineDto>({
      method: 'POST',
      url: '/api/erp/phys-invt-order-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/phys-invt-order-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PhysInvtOrderLineDto>({
      method: 'GET',
      url: `/api/erp/phys-invt-order-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPhysInvtOrderLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PhysInvtOrderLineDto>>({
      method: 'GET',
      url: '/api/erp/phys-invt-order-line',
      params: { dynamicFilter: input.dynamicFilter, physInvtOrderHeaderId: input.physInvtOrderHeaderId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePhysInvtOrderLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PhysInvtOrderLineDto>({
      method: 'PUT',
      url: `/api/erp/phys-invt-order-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
