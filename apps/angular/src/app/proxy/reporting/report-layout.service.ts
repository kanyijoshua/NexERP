import type {
  CreateUpdateReportLayoutDto,
  GetReportLayoutsInput,
  PreviewReportLayoutInput,
  ReportLayoutDetailDto,
  ReportLayoutDto,
  ReportNameDto,
  SetDefaultReportLayoutInput,
} from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ReportLayoutService {
  apiName = 'Erp';

  create = (input: CreateUpdateReportLayoutDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportLayoutDto>(
      {
        method: 'POST',
        url: '/api/erp/report-layout',
        body: input,
      },
      { apiName: this.apiName, ...config },
    );

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/erp/report-layout/${id}`,
      },
      { apiName: this.apiName, ...config },
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportLayoutDetailDto>(
      {
        method: 'GET',
        url: `/api/erp/report-layout/${id}`,
      },
      { apiName: this.apiName, ...config },
    );

  getBuiltInTemplate = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, string>(
      {
        method: 'GET',
        responseType: 'text',
        url: '/api/erp/report-layout/built-in-template',
      },
      { apiName: this.apiName, ...config },
    );

  getList = (input: GetReportLayoutsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ReportLayoutDto>>(
      {
        method: 'GET',
        url: '/api/erp/report-layout',
        params: { reportName: input.reportName },
      },
      { apiName: this.apiName, ...config },
    );

  getReportNames = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ReportNameDto>>(
      {
        method: 'GET',
        url: '/api/erp/report-layout/report-names',
      },
      { apiName: this.apiName, ...config },
    );

  runPreview = (input: PreviewReportLayoutInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, string>(
      {
        method: 'POST',
        responseType: 'text',
        url: '/api/erp/report-layout/run-preview',
        body: input,
      },
      { apiName: this.apiName, ...config },
    );

  setDefault = (input: SetDefaultReportLayoutInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'POST',
        url: '/api/erp/report-layout/set-default',
        body: input,
      },
      { apiName: this.apiName, ...config },
    );

  update = (id: string, input: CreateUpdateReportLayoutDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportLayoutDto>(
      {
        method: 'PUT',
        url: `/api/erp/report-layout/${id}`,
        body: input,
      },
      { apiName: this.apiName, ...config },
    );

  constructor(private restService: RestService) {}
}
