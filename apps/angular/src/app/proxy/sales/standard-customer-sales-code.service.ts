import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateStandardCustomerSalesCodeDto, GetStandardCustomerSalesCodeListInput, StandardCustomerSalesCodeDto } from './standard-codes.models';

@Injectable({
  providedIn: 'root',
})
export class StandardCustomerSalesCodeService {
  apiName = 'Erp';

  create = (input: CreateUpdateStandardCustomerSalesCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardCustomerSalesCodeDto>({
      method: 'POST',
      url: '/api/erp/standard-customer-sales-code',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/standard-customer-sales-code/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardCustomerSalesCodeDto>({
      method: 'GET',
      url: `/api/erp/standard-customer-sales-code/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetStandardCustomerSalesCodeListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StandardCustomerSalesCodeDto>>({
      method: 'GET',
      url: '/api/erp/standard-customer-sales-code',
      params: { dynamicFilter: input.dynamicFilter, customerNo: input.customerNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateStandardCustomerSalesCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardCustomerSalesCodeDto>({
      method: 'PUT',
      url: `/api/erp/standard-customer-sales-code/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
