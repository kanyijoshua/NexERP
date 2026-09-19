import type { CreateUpdateUnitOfMeasureDto, UnitOfMeasureDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class unit-of-measureService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateUnitOfMeasureDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, UnitOfMeasureDto>({
      method: 'POST',
      url: '/api/erp/unit-of-measure',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/unit-of-measure/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, UnitOfMeasureDto>({
      method: 'GET',
      url: `/api/erp/unit-of-measure/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<UnitOfMeasureDto>>({
      method: 'GET',
      url: '/api/erp/unit-of-measure',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateUnitOfMeasureDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, UnitOfMeasureDto>({
      method: 'PUT',
      url: `/api/erp/unit-of-measure/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
