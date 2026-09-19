import type { CreateDimensionDto, CreateDimensionValueDto, DimensionDto, DimensionValueDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class dimensionService {
  apiName = 'Erp';
  

  create = (input: CreateDimensionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DimensionDto>({
      method: 'POST',
      url: '/api/erp/dimension',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createValue = (input: CreateDimensionValueDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DimensionValueDto>({
      method: 'POST',
      url: '/api/erp/dimension/value',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<DimensionDto>>({
      method: 'GET',
      url: '/api/erp/dimension',
    },
    { apiName: this.apiName,...config });
  

  getValues = (dimensionId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<DimensionValueDto>>({
      method: 'GET',
      url: `/api/erp/dimension/values/${dimensionId}`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
