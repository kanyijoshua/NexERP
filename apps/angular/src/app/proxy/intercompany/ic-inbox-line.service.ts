import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetICInboxLineListInput, ICInboxLineDto } from './intercompany.models';

@Injectable({
  providedIn: 'root',
})
export class ICInboxLineService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ICInboxLineDto>({
      method: 'GET',
      url: `/api/erp/ic-inbox-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetICInboxLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ICInboxLineDto>>({
      method: 'GET',
      url: '/api/erp/ic-inbox-line',
      params: { dynamicFilter: input.dynamicFilter, icInboxTransactionId: input.icInboxTransactionId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
