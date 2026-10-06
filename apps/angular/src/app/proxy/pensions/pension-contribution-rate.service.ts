import type { CreateUpdatePensionContributionRateDto, GetPensionContributionRateListInput, PensionContributionRateDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionContributionRateService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionContributionRateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionRateDto>({
      method: 'POST',
      url: '/api/erp/pension-contribution-rate',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-contribution-rate/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionRateDto>({
      method: 'GET',
      url: `/api/erp/pension-contribution-rate/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionContributionRateListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionContributionRateDto>>({
      method: 'GET',
      url: '/api/erp/pension-contribution-rate',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sponsorNo: input.sponsorNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionContributionRateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionRateDto>({
      method: 'PUT',
      url: `/api/erp/pension-contribution-rate/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
