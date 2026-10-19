import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetPostedTransferListInput, PostedTransferDto } from './warehouse.models';

@Injectable({
  providedIn: 'root',
})
export class PostedTransferService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostedTransferDto>({
      method: 'GET',
      url: `/api/erp/posted-transfer/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPostedTransferListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PostedTransferDto>>({
      method: 'GET',
      url: '/api/erp/posted-transfer',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, receipt: input.receipt, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
