import type { CreateUpdateLaundryOrderLineDto, GetDocumentLineListInput, LaundryOrderLineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LaundryOrderLineService {
  apiName = 'Erp';


  create = (input: CreateUpdateLaundryOrderLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryOrderLineDto>({
      method: 'POST',
      url: '/api/erp/laundry-order-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/laundry-order-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryOrderLineDto>({
      method: 'GET',
      url: `/api/erp/laundry-order-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetDocumentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LaundryOrderLineDto>>({
      method: 'GET',
      url: '/api/erp/laundry-order-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateLaundryOrderLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LaundryOrderLineDto>({
      method: 'PUT',
      url: `/api/erp/laundry-order-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
