import type { CreateUpdatePaymentTermsDto, PaymentTermsDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PaymentTermsService {
  apiName = 'Erp';


  create = (input: CreateUpdatePaymentTermsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentTermsDto>({
      method: 'POST',
      url: '/api/erp/payment-terms',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/payment-terms/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentTermsDto>({
      method: 'GET',
      url: `/api/erp/payment-terms/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PaymentTermsDto>>({
      method: 'GET',
      url: '/api/erp/payment-terms',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePaymentTermsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentTermsDto>({
      method: 'PUT',
      url: `/api/erp/payment-terms/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
