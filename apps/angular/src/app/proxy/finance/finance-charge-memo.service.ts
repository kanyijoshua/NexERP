import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CollectionDocumentDto, GetCollectionDocumentListInput, IssueCollectionDocumentInput, SuggestCollectionDocumentsInput, UpdateCollectionDocumentDto } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class FinanceChargeMemoService {
  apiName = 'Erp';

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/finance-charge-memo/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CollectionDocumentDto>({
      method: 'GET',
      url: `/api/erp/finance-charge-memo/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCollectionDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CollectionDocumentDto>>({
      method: 'GET',
      url: '/api/erp/finance-charge-memo',
      params: { filter: input.filter, customerNo: input.customerNo, status: input.status, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: UpdateCollectionDocumentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CollectionDocumentDto>({
      method: 'PUT',
      url: `/api/erp/finance-charge-memo/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  suggest = (input: SuggestCollectionDocumentsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: '/api/erp/finance-charge-memo/suggest',
      body: input,
    },
    { apiName: this.apiName,...config });

  issue = (id: string, input: IssueCollectionDocumentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CollectionDocumentDto>({
      method: 'POST',
      url: `/api/erp/finance-charge-memo/${id}/issue`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
