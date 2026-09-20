import type { CreateUpdateWebServiceDto, PublishedWebServiceDto, SetWebServicePublishedInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WebServiceService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateWebServiceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublishedWebServiceDto>({
      method: 'POST',
      url: '/api/erp/web-service',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/web-service/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<PublishedWebServiceDto>>({
      method: 'GET',
      url: '/api/erp/web-service',
    },
    { apiName: this.apiName,...config });
  

  setPublished = (input: SetWebServicePublishedInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublishedWebServiceDto>({
      method: 'POST',
      url: '/api/erp/web-service/set-published',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateWebServiceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PublishedWebServiceDto>({
      method: 'PUT',
      url: `/api/erp/web-service/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
