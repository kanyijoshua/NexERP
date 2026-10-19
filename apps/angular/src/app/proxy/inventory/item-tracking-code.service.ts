import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateItemTrackingCodeDto, ItemTrackingCodeDto } from './warehouse.models';
import type { GetCodeTableListInput } from '../companies/models';

@Injectable({
  providedIn: 'root',
})
export class ItemTrackingCodeService {
  apiName = 'Erp';

  create = (input: CreateUpdateItemTrackingCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemTrackingCodeDto>({
      method: 'POST',
      url: '/api/erp/item-tracking-code',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-tracking-code/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemTrackingCodeDto>({
      method: 'GET',
      url: `/api/erp/item-tracking-code/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemTrackingCodeDto>>({
      method: 'GET',
      url: '/api/erp/item-tracking-code',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateItemTrackingCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemTrackingCodeDto>({
      method: 'PUT',
      url: `/api/erp/item-tracking-code/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
