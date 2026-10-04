import type { CreateUpdatePensionContributionHeaderDto, GetPensionContributionListInput, PensionContributionHeaderDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionContributionService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionContributionHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionHeaderDto>({
      method: 'POST',
      url: '/api/erp/pension-contribution',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-contribution/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionHeaderDto>({
      method: 'GET',
      url: `/api/erp/pension-contribution/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionContributionListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionContributionHeaderDto>>({
      method: 'GET',
      url: '/api/erp/pension-contribution',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sponsorNo: input.sponsorNo, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionContributionHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionHeaderDto>({
      method: 'PUT',
      url: `/api/erp/pension-contribution/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  release = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionHeaderDto>({
      method: 'POST',
      url: `/api/erp/pension-contribution/${id}/release`,
    },
    { apiName: this.apiName,...config });


  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionHeaderDto>({
      method: 'POST',
      url: `/api/erp/pension-contribution/${id}/reopen`,
    },
    { apiName: this.apiName,...config });


  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionHeaderDto>({
      method: 'POST',
      url: `/api/erp/pension-contribution/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });


  suggestLines = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionHeaderDto>({
      method: 'POST',
      url: `/api/erp/pension-contribution/${id}/suggest-lines`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
