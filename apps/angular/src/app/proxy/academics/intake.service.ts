import type { CreateUpdateIntakeDto, IntakeDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class IntakeService {
  apiName = 'Erp';


  create = (input: CreateUpdateIntakeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IntakeDto>({
      method: 'POST',
      url: '/api/erp/intake',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/intake/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IntakeDto>({
      method: 'GET',
      url: `/api/erp/intake/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<IntakeDto>>({
      method: 'GET',
      url: '/api/erp/intake',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateIntakeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IntakeDto>({
      method: 'PUT',
      url: `/api/erp/intake/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
