import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateDeferralTemplateDto, DeferralPeriodDto, DeferralTemplateDto, PreviewDeferralInput } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class DeferralTemplateService {
  apiName = 'Erp';

  create = (input: CreateUpdateDeferralTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DeferralTemplateDto>({
      method: 'POST',
      url: '/api/erp/deferral-template',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/deferral-template/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DeferralTemplateDto>({
      method: 'GET',
      url: `/api/erp/deferral-template/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<DeferralTemplateDto>>({
      method: 'GET',
      url: '/api/erp/deferral-template',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateDeferralTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DeferralTemplateDto>({
      method: 'PUT',
      url: `/api/erp/deferral-template/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  previewSchedule = (input: PreviewDeferralInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DeferralPeriodDto[]>({
      method: 'POST',
      url: '/api/erp/deferral-template/preview-schedule',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
