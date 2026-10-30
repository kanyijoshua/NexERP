import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateQualityControlPointDto, QualityControlPointDto } from './quality.models';

@Injectable({
  providedIn: 'root',
})
export class QualityControlPointService {
  apiName = 'Erp';

  create = (input: CreateUpdateQualityControlPointDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, QualityControlPointDto>({
      method: 'POST',
      url: '/api/erp/quality-control-point',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/quality-control-point/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, QualityControlPointDto>({
      method: 'GET',
      url: `/api/erp/quality-control-point/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<QualityControlPointDto>>({
      method: 'GET',
      url: '/api/erp/quality-control-point',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateQualityControlPointDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, QualityControlPointDto>({
      method: 'PUT',
      url: `/api/erp/quality-control-point/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
