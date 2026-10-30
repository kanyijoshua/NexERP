import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetPosOrderLineListInput, PosOrderLineDto } from './point-of-sale.models';

@Injectable({
  providedIn: 'root',
})
export class PosOrderLineService {
  apiName = 'Erp';

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pos-order-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosOrderLineDto>({
      method: 'GET',
      url: `/api/erp/pos-order-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPosOrderLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PosOrderLineDto>>({
      method: 'GET',
      url: '/api/erp/pos-order-line',
      params: { dynamicFilter: input.dynamicFilter, posOrderId: input.posOrderId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
