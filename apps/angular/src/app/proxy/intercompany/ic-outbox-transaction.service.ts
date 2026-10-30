import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetICTransactionListInput, ICOutboxTransactionDto, ICSendResultDto } from './intercompany.models';

@Injectable({
  providedIn: 'root',
})
export class ICOutboxTransactionService {
  apiName = 'Erp';

  cancel = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/ic-outbox-transaction/${id}/cancel`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ICOutboxTransactionDto>({
      method: 'GET',
      url: `/api/erp/ic-outbox-transaction/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetICTransactionListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ICOutboxTransactionDto>>({
      method: 'GET',
      url: '/api/erp/ic-outbox-transaction',
      params: { dynamicFilter: input.dynamicFilter, partnerCode: input.partnerCode, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  send = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ICSendResultDto>({
      method: 'POST',
      url: `/api/erp/ic-outbox-transaction/${id}/send`,
    },
    { apiName: this.apiName,...config });

  sendAll = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ICSendResultDto>({
      method: 'POST',
      url: '/api/erp/ic-outbox-transaction/send-all',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
