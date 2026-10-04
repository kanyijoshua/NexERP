import type { CreateUpdateMiscArticleInformationDto, GetMiscArticleInformationListInput, MiscArticleInformationDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MiscArticleInformationService {
  apiName = 'Erp';


  create = (input: CreateUpdateMiscArticleInformationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MiscArticleInformationDto>({
      method: 'POST',
      url: '/api/erp/misc-article-information',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/misc-article-information/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MiscArticleInformationDto>({
      method: 'GET',
      url: `/api/erp/misc-article-information/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetMiscArticleInformationListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MiscArticleInformationDto>>({
      method: 'GET',
      url: '/api/erp/misc-article-information',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, employeeNo: input.employeeNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateMiscArticleInformationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MiscArticleInformationDto>({
      method: 'PUT',
      url: `/api/erp/misc-article-information/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
