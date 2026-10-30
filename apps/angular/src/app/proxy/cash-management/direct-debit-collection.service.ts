import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { BankFileDto, DirectDebitCollectionDto, DirectDebitReceiptLinesResultDto, GetDirectDebitCollectionListInput, MakeDirectDebitCollectionInput, MakeDirectDebitReceiptLinesInput } from './payment-export.models';

@Injectable({
  providedIn: 'root',
})
export class DirectDebitCollectionService {
  apiName = 'Erp';

  close = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DirectDebitCollectionDto>({
      method: 'POST',
      url: `/api/erp/direct-debit-collection/${id}/close`,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/direct-debit-collection/${id}`,
    },
    { apiName: this.apiName,...config });

  export = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BankFileDto>({
      method: 'POST',
      url: `/api/erp/direct-debit-collection/${id}/export`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DirectDebitCollectionDto>({
      method: 'GET',
      url: `/api/erp/direct-debit-collection/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetDirectDebitCollectionListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<DirectDebitCollectionDto>>({
      method: 'GET',
      url: '/api/erp/direct-debit-collection',
      params: { dynamicFilter: input.dynamicFilter, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  make = (input: MakeDirectDebitCollectionInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DirectDebitCollectionDto>({
      method: 'POST',
      url: '/api/erp/direct-debit-collection/make',
      body: input,
    },
    { apiName: this.apiName,...config });

  makeReceiptLines = (id: string, input: MakeDirectDebitReceiptLinesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DirectDebitReceiptLinesResultDto>({
      method: 'POST',
      url: `/api/erp/direct-debit-collection/${id}/make-receipt-lines`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
