import type { CreateUpdateHumanResourceUnitOfMeasureDto, HumanResourceUnitOfMeasureDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class HumanResourceUnitOfMeasureService {
  apiName = 'Erp';


  create = (input: CreateUpdateHumanResourceUnitOfMeasureDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HumanResourceUnitOfMeasureDto>({
      method: 'POST',
      url: '/api/erp/human-resource-unit-of-measure',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/human-resource-unit-of-measure/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HumanResourceUnitOfMeasureDto>({
      method: 'GET',
      url: `/api/erp/human-resource-unit-of-measure/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<HumanResourceUnitOfMeasureDto>>({
      method: 'GET',
      url: '/api/erp/human-resource-unit-of-measure',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateHumanResourceUnitOfMeasureDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HumanResourceUnitOfMeasureDto>({
      method: 'PUT',
      url: `/api/erp/human-resource-unit-of-measure/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
