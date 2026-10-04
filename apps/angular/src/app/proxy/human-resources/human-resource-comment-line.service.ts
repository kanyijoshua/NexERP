import type { CreateUpdateHumanResourceCommentLineDto, GetHumanResourceCommentLineListInput, HumanResourceCommentLineDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class HumanResourceCommentLineService {
  apiName = 'Erp';


  create = (input: CreateUpdateHumanResourceCommentLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HumanResourceCommentLineDto>({
      method: 'POST',
      url: '/api/erp/human-resource-comment-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/human-resource-comment-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HumanResourceCommentLineDto>({
      method: 'GET',
      url: `/api/erp/human-resource-comment-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetHumanResourceCommentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<HumanResourceCommentLineDto>>({
      method: 'GET',
      url: '/api/erp/human-resource-comment-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, no: input.no, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateHumanResourceCommentLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HumanResourceCommentLineDto>({
      method: 'PUT',
      url: `/api/erp/human-resource-comment-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
