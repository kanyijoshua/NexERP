import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateDirectDebitMandateDto, DirectDebitMandateDto, GetDirectDebitMandateListInput } from './payment-export.models';

@Injectable({
  providedIn: 'root',
})
export class DirectDebitMandateService {
  apiName = 'Erp';

  create = (input: CreateUpdateDirectDebitMandateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DirectDebitMandateDto>({
      method: 'POST',
      url: '/api/erp/direct-debit-mandate',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/direct-debit-mandate/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DirectDebitMandateDto>({
      method: 'GET',
      url: `/api/erp/direct-debit-mandate/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetDirectDebitMandateListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<DirectDebitMandateDto>>({
      method: 'GET',
      url: '/api/erp/direct-debit-mandate',
      params: { dynamicFilter: input.dynamicFilter, customerNo: input.customerNo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateDirectDebitMandateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DirectDebitMandateDto>({
      method: 'PUT',
      url: `/api/erp/direct-debit-mandate/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
