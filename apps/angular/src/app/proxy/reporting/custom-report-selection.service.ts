import type { CreateUpdateCustomReportSelectionDto, CustomReportSelectionDto, GetCustomReportSelectionListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CustomReportSelectionService {
  apiName = 'Erp';


  create = (input: CreateUpdateCustomReportSelectionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomReportSelectionDto>({
      method: 'POST',
      url: '/api/erp/custom-report-selection',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/custom-report-selection/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomReportSelectionDto>({
      method: 'GET',
      url: `/api/erp/custom-report-selection/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCustomReportSelectionListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CustomReportSelectionDto>>({
      method: 'GET',
      url: '/api/erp/custom-report-selection',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sourceNo: input.sourceNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateCustomReportSelectionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomReportSelectionDto>({
      method: 'PUT',
      url: `/api/erp/custom-report-selection/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
