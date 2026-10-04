import type { CreateUpdatePurchCommentLineDto, GetPurchCommentLineListInput, PurchCommentLineDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PurchCommentLineService {
  apiName = 'Erp';


  create = (input: CreateUpdatePurchCommentLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchCommentLineDto>({
      method: 'POST',
      url: '/api/erp/purch-comment-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/purch-comment-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchCommentLineDto>({
      method: 'GET',
      url: `/api/erp/purch-comment-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPurchCommentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PurchCommentLineDto>>({
      method: 'GET',
      url: '/api/erp/purch-comment-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, no: input.no, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePurchCommentLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchCommentLineDto>({
      method: 'PUT',
      url: `/api/erp/purch-comment-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
