import type { CreateUpdateReportSelectionDto, GetReportSelectionListInput, ReportSelectionDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ReportSelectionService {
  apiName = 'Erp';


  create = (input: CreateUpdateReportSelectionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportSelectionDto>({
      method: 'POST',
      url: '/api/erp/report-selection',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/report-selection/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportSelectionDto>({
      method: 'GET',
      url: `/api/erp/report-selection/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetReportSelectionListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ReportSelectionDto>>({
      method: 'GET',
      url: '/api/erp/report-selection',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateReportSelectionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportSelectionDto>({
      method: 'PUT',
      url: `/api/erp/report-selection/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
