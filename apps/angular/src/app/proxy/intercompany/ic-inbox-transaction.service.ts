import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { AcceptICInboxInput, GetICTransactionListInput, ICAcceptResultDto, ICInboxTransactionDto } from './intercompany.models';

@Injectable({
  providedIn: 'root',
})
export class ICInboxTransactionService {
  apiName = 'Erp';

  accept = (id: string, input: AcceptICInboxInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ICAcceptResultDto>({
      method: 'POST',
      url: `/api/erp/ic-inbox-transaction/${id}/accept`,
      body: input,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ICInboxTransactionDto>({
      method: 'GET',
      url: `/api/erp/ic-inbox-transaction/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetICTransactionListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ICInboxTransactionDto>>({
      method: 'GET',
      url: '/api/erp/ic-inbox-transaction',
      params: { dynamicFilter: input.dynamicFilter, partnerCode: input.partnerCode, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  reject = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/ic-inbox-transaction/${id}/reject`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
