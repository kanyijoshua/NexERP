import type { CreateUpdateResponsibilityCenterDto, ResponsibilityCenterDto } from './base-tables.models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ResponsibilityCenterService {
  apiName = 'Erp';


  create = (input: CreateUpdateResponsibilityCenterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ResponsibilityCenterDto>({
      method: 'POST',
      url: '/api/erp/responsibility-center',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/responsibility-center/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ResponsibilityCenterDto>({
      method: 'GET',
      url: `/api/erp/responsibility-center/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ResponsibilityCenterDto>>({
      method: 'GET',
      url: '/api/erp/responsibility-center',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateResponsibilityCenterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ResponsibilityCenterDto>({
      method: 'PUT',
      url: `/api/erp/responsibility-center/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
