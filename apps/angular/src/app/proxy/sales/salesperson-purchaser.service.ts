import type { CreateUpdateSalespersonPurchaserDto, SalespersonPurchaserDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SalespersonPurchaserService {
  apiName = 'Erp';


  create = (input: CreateUpdateSalespersonPurchaserDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalespersonPurchaserDto>({
      method: 'POST',
      url: '/api/erp/salesperson-purchaser',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/salesperson-purchaser/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalespersonPurchaserDto>({
      method: 'GET',
      url: `/api/erp/salesperson-purchaser/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SalespersonPurchaserDto>>({
      method: 'GET',
      url: '/api/erp/salesperson-purchaser',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateSalespersonPurchaserDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalespersonPurchaserDto>({
      method: 'PUT',
      url: `/api/erp/salesperson-purchaser/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
