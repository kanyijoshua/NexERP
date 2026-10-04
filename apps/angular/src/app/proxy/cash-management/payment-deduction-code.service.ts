import type { CreateUpdatePaymentDeductionCodeDto, PaymentDeductionCodeDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PaymentDeductionCodeService {
  apiName = 'Erp';


  create = (input: CreateUpdatePaymentDeductionCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentDeductionCodeDto>({
      method: 'POST',
      url: '/api/erp/payment-deduction-code',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/payment-deduction-code/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentDeductionCodeDto>({
      method: 'GET',
      url: `/api/erp/payment-deduction-code/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PaymentDeductionCodeDto>>({
      method: 'GET',
      url: '/api/erp/payment-deduction-code',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePaymentDeductionCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PaymentDeductionCodeDto>({
      method: 'PUT',
      url: `/api/erp/payment-deduction-code/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
