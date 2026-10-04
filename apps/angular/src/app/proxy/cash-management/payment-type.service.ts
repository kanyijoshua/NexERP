import type { CreateUpdatePaymentTypeDto, PaymentTypeDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PaymentTypeService {
  apiName = 'Erp';


  create = (input: CreateUpdatePaymentTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentTypeDto>({
      method: 'POST',
      url: '/api/erp/payment-type',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/payment-type/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentTypeDto>({
      method: 'GET',
      url: `/api/erp/payment-type/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PaymentTypeDto>>({
      method: 'GET',
      url: '/api/erp/payment-type',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePaymentTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentTypeDto>({
      method: 'PUT',
      url: `/api/erp/payment-type/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
