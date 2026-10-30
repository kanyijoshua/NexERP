import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { BusinessUnitDto, ConsolidationLineDto, ConsolidationPeriodInput, CreateUpdateBusinessUnitDto, RunConsolidationInput } from './consolidation.models';

@Injectable({
  providedIn: 'root',
})
export class BusinessUnitService {
  apiName = 'Erp';

  create = (input: CreateUpdateBusinessUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BusinessUnitDto>({
      method: 'POST',
      url: '/api/erp/business-unit',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/business-unit/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BusinessUnitDto>({
      method: 'GET',
      url: `/api/erp/business-unit/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BusinessUnitDto>>({
      method: 'GET',
      url: '/api/erp/business-unit',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  getPreview = (id: string, input: ConsolidationPeriodInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConsolidationLineDto[]>({
      method: 'GET',
      url: `/api/erp/business-unit/${id}/preview`,
      params: { startingDate: input.startingDate, endingDate: input.endingDate },
    },
    { apiName: this.apiName,...config });

  runConsolidation = (input: RunConsolidationInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConsolidationLineDto[]>({
      method: 'POST',
      url: '/api/erp/business-unit/run-consolidation',
      body: input,
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateBusinessUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BusinessUnitDto>({
      method: 'PUT',
      url: `/api/erp/business-unit/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
