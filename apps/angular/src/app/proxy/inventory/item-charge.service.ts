import type { CreateUpdateItemChargeDto, ItemChargeDto } from './item-charge.models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ItemChargeService {
  apiName = 'Erp';


  create = (input: CreateUpdateItemChargeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemChargeDto>({
      method: 'POST',
      url: '/api/erp/item-charge',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-charge/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemChargeDto>({
      method: 'GET',
      url: `/api/erp/item-charge/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemChargeDto>>({
      method: 'GET',
      url: '/api/erp/item-charge',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateItemChargeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemChargeDto>({
      method: 'PUT',
      url: `/api/erp/item-charge/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
