import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateItemTrackingLineDto, GetItemTrackingLineListInput, ItemTrackingLineDto } from './warehouse.models';

@Injectable({
  providedIn: 'root',
})
export class ItemTrackingLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateItemTrackingLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemTrackingLineDto>({
      method: 'POST',
      url: '/api/erp/item-tracking-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/item-tracking-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemTrackingLineDto>({
      method: 'GET',
      url: `/api/erp/item-tracking-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetItemTrackingLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ItemTrackingLineDto>>({
      method: 'GET',
      url: '/api/erp/item-tracking-line',
      params: { dynamicFilter: input.dynamicFilter, sourceType: input.sourceType, sourceId: input.sourceId, itemNo: input.itemNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateItemTrackingLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ItemTrackingLineDto>({
      method: 'PUT',
      url: `/api/erp/item-tracking-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
