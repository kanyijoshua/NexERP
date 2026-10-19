import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { FABookValueDto } from './fixed-asset-posting.models';

@Injectable({
  providedIn: 'root',
})
export class FixedAssetValueService {
  apiName = 'Erp';

  getBookValues = (input: { faNo: string }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FABookValueDto[]>({
      method: 'GET',
      url: '/api/erp/fixed-asset-value/book-values',
      params: { faNo: input.faNo },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
