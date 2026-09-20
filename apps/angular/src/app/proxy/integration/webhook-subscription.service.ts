import type { CreateUpdateWebhookSubscriptionDto, GetWebhookDeliveriesInput, WebhookDeliveryDto, WebhookSubscriptionDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WebhookSubscriptionService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateWebhookSubscriptionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WebhookSubscriptionDto>({
      method: 'POST',
      url: '/api/erp/webhook-subscription',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/webhook-subscription/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getDeliveries = (input: GetWebhookDeliveriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WebhookDeliveryDto>>({
      method: 'GET',
      url: '/api/erp/webhook-subscription/deliveries',
      params: { subscriptionId: input.subscriptionId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<WebhookSubscriptionDto>>({
      method: 'GET',
      url: '/api/erp/webhook-subscription',
    },
    { apiName: this.apiName,...config });
  

  regenerateSecret = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WebhookSubscriptionDto>({
      method: 'POST',
      url: `/api/erp/webhook-subscription/${id}/regenerate-secret`,
    },
    { apiName: this.apiName,...config });
  

  retryDelivery = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WebhookDeliveryDto>({
      method: 'POST',
      url: `/api/erp/webhook-subscription/${id}/retry-delivery`,
    },
    { apiName: this.apiName,...config });
  

  sendTest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WebhookDeliveryDto>({
      method: 'POST',
      url: `/api/erp/webhook-subscription/${id}/send-test`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateWebhookSubscriptionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WebhookSubscriptionDto>({
      method: 'PUT',
      url: `/api/erp/webhook-subscription/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
