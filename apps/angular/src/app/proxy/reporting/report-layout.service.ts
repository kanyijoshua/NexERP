import type { CreateReportLayoutDto, GetReportLayoutsInput, ReportLayoutDto, SetDefaultReportLayoutInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ReportLayoutService {
  apiName = 'Erp';
  

  create = (input: CreateReportLayoutDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportLayoutDto>({
      method: 'POST',
      url: '/api/erp/report-layout',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetReportLayoutsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ReportLayoutDto>>({
      method: 'GET',
      url: '/api/erp/report-layout',
      params: { reportName: input.reportName },
    },
    { apiName: this.apiName,...config });
  

  setDefault = (input: SetDefaultReportLayoutInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/erp/report-layout/set-default',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
