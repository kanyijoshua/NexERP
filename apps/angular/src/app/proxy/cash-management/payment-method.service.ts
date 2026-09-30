import type { CreateUpdatePaymentMethodDto, PaymentMethodDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PaymentMethodService {
  apiName = 'Erp';


  create = (input: CreateUpdatePaymentMethodDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentMethodDto>({
      method: 'POST',
      url: '/api/erp/payment-method',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/payment-method/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentMethodDto>({
      method: 'GET',
      url: `/api/erp/payment-method/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PaymentMethodDto>>({
      method: 'GET',
      url: '/api/erp/payment-method',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePaymentMethodDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentMethodDto>({
      method: 'PUT',
      url: `/api/erp/payment-method/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
