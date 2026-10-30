import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdatePhysInvtCountingPeriodDto, PhysInvtCountingPeriodDto } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class PhysInvtCountingPeriodService {
  apiName = 'Erp';

  create = (input: CreateUpdatePhysInvtCountingPeriodDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PhysInvtCountingPeriodDto>({
      method: 'POST',
      url: '/api/erp/phys-invt-counting-period',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/phys-invt-counting-period/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PhysInvtCountingPeriodDto>({
      method: 'GET',
      url: `/api/erp/phys-invt-counting-period/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PhysInvtCountingPeriodDto>>({
      method: 'GET',
      url: '/api/erp/phys-invt-counting-period',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePhysInvtCountingPeriodDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PhysInvtCountingPeriodDto>({
      method: 'PUT',
      url: `/api/erp/phys-invt-counting-period/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
