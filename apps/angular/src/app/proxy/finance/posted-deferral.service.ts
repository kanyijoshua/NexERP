import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetPostedDeferralListInput, PostedDeferralHeaderDto } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class PostedDeferralService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PostedDeferralHeaderDto>({
      method: 'GET',
      url: `/api/erp/posted-deferral/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPostedDeferralListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PostedDeferralHeaderDto>>({
      method: 'GET',
      url: '/api/erp/posted-deferral',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
