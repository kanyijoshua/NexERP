import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { DirectDebitCollectionEntryDto, GetDirectDebitCollectionEntryListInput } from './payment-export.models';

@Injectable({
  providedIn: 'root',
})
export class DirectDebitCollectionEntryService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DirectDebitCollectionEntryDto>({
      method: 'GET',
      url: `/api/erp/direct-debit-collection-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetDirectDebitCollectionEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<DirectDebitCollectionEntryDto>>({
      method: 'GET',
      url: '/api/erp/direct-debit-collection-entry',
      params: { dynamicFilter: input.dynamicFilter, directDebitCollectionId: input.directDebitCollectionId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
