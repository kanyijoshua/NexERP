import type { CreateUpdateLaundryItemDto, LaundryItemDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LaundryItemService {
  apiName = 'Erp';


  create = (input: CreateUpdateLaundryItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryItemDto>({
      method: 'POST',
      url: '/api/erp/laundry-item',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/laundry-item/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryItemDto>({
      method: 'GET',
      url: `/api/erp/laundry-item/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LaundryItemDto>>({
      method: 'GET',
      url: '/api/erp/laundry-item',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateLaundryItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryItemDto>({
      method: 'PUT',
      url: `/api/erp/laundry-item/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
