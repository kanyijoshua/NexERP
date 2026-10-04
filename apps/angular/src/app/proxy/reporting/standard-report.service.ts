import type { ReportResultDto, RunStandardReportInput, StandardReportDto, StandardReportExportInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StandardReportService {
  apiName = 'Erp';


  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardReportDto[]>({
      method: 'GET',
      url: '/api/erp/standard-report',
    },
    { apiName: this.apiName,...config });


  run = (input: RunStandardReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportResultDto>({
      method: 'POST',
      url: '/api/erp/standard-report/run',
      body: input,
    },
    { apiName: this.apiName,...config });


  runExport = (input: StandardReportExportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/erp/standard-report/run-export',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
