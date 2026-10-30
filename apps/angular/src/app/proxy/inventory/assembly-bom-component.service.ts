import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { AssemblyBomComponentDto, CreateUpdateAssemblyBomComponentDto, GetAssemblyBomComponentListInput } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class AssemblyBomComponentService {
  apiName = 'Erp';

  create = (input: CreateUpdateAssemblyBomComponentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyBomComponentDto>({
      method: 'POST',
      url: '/api/erp/assembly-bom-component',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/assembly-bom-component/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyBomComponentDto>({
      method: 'GET',
      url: `/api/erp/assembly-bom-component/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetAssemblyBomComponentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AssemblyBomComponentDto>>({
      method: 'GET',
      url: '/api/erp/assembly-bom-component',
      params: { dynamicFilter: input.dynamicFilter, parentItemNo: input.parentItemNo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateAssemblyBomComponentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyBomComponentDto>({
      method: 'PUT',
      url: `/api/erp/assembly-bom-component/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
