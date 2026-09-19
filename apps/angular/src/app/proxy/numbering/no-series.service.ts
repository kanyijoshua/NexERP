import type { CreateUpdateNoSeriesDto, GetNoSeriesListInput, NextNoPreviewDto, NextNoPreviewInput, NoSeriesDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class NoSeriesService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateNoSeriesDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, NoSeriesDto>({
      method: 'POST',
      url: '/api/erp/no-series',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/no-series/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, NoSeriesDto>({
      method: 'GET',
      url: `/api/erp/no-series/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetNoSeriesListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<NoSeriesDto>>({
      method: 'GET',
      url: '/api/erp/no-series',
      params: { filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getNextNoPreview = (input: NextNoPreviewInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, NextNoPreviewDto>({
      method: 'GET',
      url: '/api/erp/no-series/next-no-preview',
      params: { code: input.code, date: input.date },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateNoSeriesDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, NoSeriesDto>({
      method: 'PUT',
      url: `/api/erp/no-series/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
