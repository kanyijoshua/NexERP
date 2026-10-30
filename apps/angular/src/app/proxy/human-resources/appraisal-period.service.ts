import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { AppraisalPeriodDto, CreateUpdateAppraisalPeriodDto } from './recruitment-appraisals.models';

@Injectable({
  providedIn: 'root',
})
export class AppraisalPeriodService {
  apiName = 'Erp';

  create = (input: CreateUpdateAppraisalPeriodDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalPeriodDto>({
      method: 'POST',
      url: '/api/erp/appraisal-period',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/appraisal-period/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalPeriodDto>({
      method: 'GET',
      url: `/api/erp/appraisal-period/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AppraisalPeriodDto>>({
      method: 'GET',
      url: '/api/erp/appraisal-period',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateAppraisalPeriodDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AppraisalPeriodDto>({
      method: 'PUT',
      url: `/api/erp/appraisal-period/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
