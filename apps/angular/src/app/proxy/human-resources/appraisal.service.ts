import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { AppraisalDto, CreateUpdateAppraisalDto, GetAppraisalListInput } from './recruitment-appraisals.models';

@Injectable({
  providedIn: 'root',
})
export class AppraisalService {
  apiName = 'Erp';

  complete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalDto>({
      method: 'POST',
      url: `/api/erp/appraisal/${id}/complete`,
    },
    { apiName: this.apiName,...config });

  create = (input: CreateUpdateAppraisalDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalDto>({
      method: 'POST',
      url: '/api/erp/appraisal',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/appraisal/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalDto>({
      method: 'GET',
      url: `/api/erp/appraisal/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetAppraisalListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AppraisalDto>>({
      method: 'GET',
      url: '/api/erp/appraisal',
      params: { dynamicFilter: input.dynamicFilter, employeeNo: input.employeeNo, periodCode: input.periodCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalDto>({
      method: 'POST',
      url: `/api/erp/appraisal/${id}/reopen`,
    },
    { apiName: this.apiName,...config });

  startSelfReview = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalDto>({
      method: 'POST',
      url: `/api/erp/appraisal/${id}/start-self-review`,
    },
    { apiName: this.apiName,...config });

  submitToReviewer = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalDto>({
      method: 'POST',
      url: `/api/erp/appraisal/${id}/submit-to-reviewer`,
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateAppraisalDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalDto>({
      method: 'PUT',
      url: `/api/erp/appraisal/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
