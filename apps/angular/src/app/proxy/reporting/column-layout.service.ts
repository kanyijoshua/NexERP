import type { ColumnLayoutDto, ColumnLayoutLineDto, CreateUpdateColumnLayoutDto, CreateUpdateColumnLayoutLineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ColumnLayoutService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateColumnLayoutDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ColumnLayoutDto>({
      method: 'POST',
      url: '/api/erp/column-layout',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createLine = (input: CreateUpdateColumnLayoutLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ColumnLayoutLineDto>({
      method: 'POST',
      url: '/api/erp/column-layout/line',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/column-layout/${id}`,
    },
    { apiName: this.apiName,...config });
  

  deleteLine = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/column-layout/${id}/line`,
    },
    { apiName: this.apiName,...config });
  

  getLines = (columnLayoutId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ColumnLayoutLineDto>>({
      method: 'GET',
      url: `/api/erp/column-layout/lines/${columnLayoutId}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ColumnLayoutDto>>({
      method: 'GET',
      url: '/api/erp/column-layout',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateColumnLayoutDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ColumnLayoutDto>({
      method: 'PUT',
      url: `/api/erp/column-layout/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateLine = (id: string, input: CreateUpdateColumnLayoutLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ColumnLayoutLineDto>({
      method: 'PUT',
      url: `/api/erp/column-layout/${id}/line`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
