import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetPosOrderListInput, PayPosOrderInput, PosOrderDto, RingUpInput, StartPosOrderInput } from './point-of-sale.models';

@Injectable({
  providedIn: 'root',
})
export class PosOrderService {
  apiName = 'Erp';

  cancel = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosOrderDto>({
      method: 'POST',
      url: `/api/erp/pos-order/${id}/cancel`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosOrderDto>({
      method: 'GET',
      url: `/api/erp/pos-order/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPosOrderListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PosOrderDto>>({
      method: 'GET',
      url: '/api/erp/pos-order',
      params: { dynamicFilter: input.dynamicFilter, sessionId: input.sessionId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  pay = (id: string, input: PayPosOrderInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosOrderDto>({
      method: 'POST',
      url: `/api/erp/pos-order/${id}/pay`,
      body: input,
    },
    { apiName: this.apiName,...config });

  ringUp = (id: string, input: RingUpInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosOrderDto>({
      method: 'POST',
      url: `/api/erp/pos-order/${id}/ring-up`,
      body: input,
    },
    { apiName: this.apiName,...config });

  start = (input: StartPosOrderInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PosOrderDto>({
      method: 'POST',
      url: '/api/erp/pos-order/start',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
