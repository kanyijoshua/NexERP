import type { CreateUpdateLaundryOrderDto, GetCampusDocumentListInput, LaundryOrderDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LaundryOrderService {
  apiName = 'Erp';


  create = (input: CreateUpdateLaundryOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryOrderDto>({
      method: 'POST',
      url: '/api/erp/laundry-order',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/laundry-order/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryOrderDto>({
      method: 'GET',
      url: `/api/erp/laundry-order/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCampusDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LaundryOrderDto>>({
      method: 'GET',
      url: '/api/erp/laundry-order',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateLaundryOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryOrderDto>({
      method: 'PUT',
      url: `/api/erp/laundry-order/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  invoice = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryOrderDto>({
      method: 'POST',
      url: `/api/erp/laundry-order/${id}/invoice`,
    },
    { apiName: this.apiName,...config });

  markReady = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryOrderDto>({
      method: 'POST',
      url: `/api/erp/laundry-order/${id}/mark-ready`,
    },
    { apiName: this.apiName,...config });

  markCollected = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryOrderDto>({
      method: 'POST',
      url: `/api/erp/laundry-order/${id}/mark-collected`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
