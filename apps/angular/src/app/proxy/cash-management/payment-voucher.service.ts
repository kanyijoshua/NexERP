import type { CreateUpdatePaymentVoucherHeaderDto, GetPaymentVoucherListInput, PaymentVoucherChequeInput, PaymentVoucherHeaderDto } from './models';
import type { ApprovalRequestResultDto } from '../workflows/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PaymentVoucherService {
  apiName = 'Erp';


  create = (input: CreateUpdatePaymentVoucherHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentVoucherHeaderDto>({
      method: 'POST',
      url: '/api/erp/payment-voucher',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/payment-voucher/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentVoucherHeaderDto>({
      method: 'GET',
      url: `/api/erp/payment-voucher/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPaymentVoucherListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PaymentVoucherHeaderDto>>({
      method: 'GET',
      url: '/api/erp/payment-voucher',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, payingBankAccountNo: input.payingBankAccountNo, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePaymentVoucherHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentVoucherHeaderDto>({
      method: 'PUT',
      url: `/api/erp/payment-voucher/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  sendApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalRequestResultDto>({
      method: 'POST',
      url: `/api/erp/payment-voucher/${id}/send-approval-request`,
    },
    { apiName: this.apiName,...config });


  cancelApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/payment-voucher/${id}/cancel-approval-request`,
    },
    { apiName: this.apiName,...config });


  release = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentVoucherHeaderDto>({
      method: 'POST',
      url: `/api/erp/payment-voucher/${id}/release`,
    },
    { apiName: this.apiName,...config });


  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentVoucherHeaderDto>({
      method: 'POST',
      url: `/api/erp/payment-voucher/${id}/reopen`,
    },
    { apiName: this.apiName,...config });


  recordCheque = (id: string, input: PaymentVoucherChequeInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentVoucherHeaderDto>({
      method: 'POST',
      url: `/api/erp/payment-voucher/${id}/record-cheque`,
      body: input,
    },
    { apiName: this.apiName,...config });


  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentVoucherHeaderDto>({
      method: 'POST',
      url: `/api/erp/payment-voucher/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
