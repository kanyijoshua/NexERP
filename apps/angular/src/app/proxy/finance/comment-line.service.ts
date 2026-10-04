import type { CommentLineDto, CreateUpdateCommentLineDto, GetCommentLineListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CommentLineService {
  apiName = 'Erp';


  create = (input: CreateUpdateCommentLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CommentLineDto>({
      method: 'POST',
      url: '/api/erp/comment-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/comment-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CommentLineDto>({
      method: 'GET',
      url: `/api/erp/comment-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCommentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CommentLineDto>>({
      method: 'GET',
      url: '/api/erp/comment-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, no: input.no, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateCommentLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CommentLineDto>({
      method: 'PUT',
      url: `/api/erp/comment-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
