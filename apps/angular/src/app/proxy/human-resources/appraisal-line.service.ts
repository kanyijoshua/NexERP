import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { AppraisalLineDto, CreateUpdateAppraisalLineDto, GetAppraisalLineListInput } from './recruitment-appraisals.models';

@Injectable({
  providedIn: 'root',
})
export class AppraisalLineService {
  apiName = 'Erp';

  create = (input: CreateUpdateAppraisalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalLineDto>({
      method: 'POST',
      url: '/api/erp/appraisal-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/appraisal-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalLineDto>({
      method: 'GET',
      url: `/api/erp/appraisal-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetAppraisalLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AppraisalLineDto>>({
      method: 'GET',
      url: '/api/erp/appraisal-line',
      params: { dynamicFilter: input.dynamicFilter, appraisalId: input.appraisalId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateAppraisalLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalLineDto>({
      method: 'PUT',
      url: `/api/erp/appraisal-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
