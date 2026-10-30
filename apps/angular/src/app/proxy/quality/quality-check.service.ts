import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateQualityCheckDto, GetQualityCheckListInput, QualityCheckDto, RecordQualityCheckInput } from './quality.models';

@Injectable({
  providedIn: 'root',
})
export class QualityCheckService {
  apiName = 'Erp';

  create = (input: CreateQualityCheckDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, QualityCheckDto>({
      method: 'POST',
      url: '/api/erp/quality-check',
      body: input,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, QualityCheckDto>({
      method: 'GET',
      url: `/api/erp/quality-check/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetQualityCheckListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<QualityCheckDto>>({
      method: 'GET',
      url: '/api/erp/quality-check',
      params: { dynamicFilter: input.dynamicFilter, sourceDocumentNo: input.sourceDocumentNo, result: input.result, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  record = (id: string, input: RecordQualityCheckInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, QualityCheckDto>({
      method: 'POST',
      url: `/api/erp/quality-check/${id}/record`,
      body: input,
    },
    { apiName: this.apiName,...config });

  reset = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, QualityCheckDto>({
      method: 'POST',
      url: `/api/erp/quality-check/${id}/reset`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
