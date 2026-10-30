import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateCustomerSubscriptionDto, CustomerSubscriptionDto, GetCustomerSubscriptionListInput, InvoiceSubscriptionsInput, SubscriptionInvoicingResultDto } from './subscriptions.models';

@Injectable({
  providedIn: 'root',
})
export class CustomerSubscriptionService {
  apiName = 'Erp';

  activate = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerSubscriptionDto>({
      method: 'POST',
      url: `/api/erp/customer-subscription/${id}/activate`,
    },
    { apiName: this.apiName,...config });

  cancel = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerSubscriptionDto>({
      method: 'POST',
      url: `/api/erp/customer-subscription/${id}/cancel`,
    },
    { apiName: this.apiName,...config });

  create = (input: CreateUpdateCustomerSubscriptionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerSubscriptionDto>({
      method: 'POST',
      url: '/api/erp/customer-subscription',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/customer-subscription/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerSubscriptionDto>({
      method: 'GET',
      url: `/api/erp/customer-subscription/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCustomerSubscriptionListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CustomerSubscriptionDto>>({
      method: 'GET',
      url: '/api/erp/customer-subscription',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  invoiceAllDue = (input: InvoiceSubscriptionsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SubscriptionInvoicingResultDto>({
      method: 'POST',
      url: '/api/erp/customer-subscription/invoice-all-due',
      body: input,
    },
    { apiName: this.apiName,...config });

  invoiceDue = (id: string, input: InvoiceSubscriptionsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SubscriptionInvoicingResultDto>({
      method: 'POST',
      url: `/api/erp/customer-subscription/${id}/invoice-due`,
      body: input,
    },
    { apiName: this.apiName,...config });

  pause = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerSubscriptionDto>({
      method: 'POST',
      url: `/api/erp/customer-subscription/${id}/pause`,
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateCustomerSubscriptionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerSubscriptionDto>({
      method: 'PUT',
      url: `/api/erp/customer-subscription/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
