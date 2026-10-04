import type { CreateUpdatePaymentVoucherLineDto, GetPaymentVoucherLineListInput, PaymentVoucherLineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PaymentVoucherLineService {
  apiName = 'Erp';


  create = (input: CreateUpdatePaymentVoucherLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentVoucherLineDto>({
      method: 'POST',
      url: '/api/erp/payment-voucher-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/payment-voucher-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentVoucherLineDto>({
      method: 'GET',
      url: `/api/erp/payment-voucher-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPaymentVoucherLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PaymentVoucherLineDto>>({
      method: 'GET',
      url: '/api/erp/payment-voucher-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePaymentVoucherLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentVoucherLineDto>({
      method: 'PUT',
      url: `/api/erp/payment-voucher-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
