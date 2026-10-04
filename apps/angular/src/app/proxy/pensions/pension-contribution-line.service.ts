import type { CreateUpdatePensionContributionLineDto, GetPensionContributionLineListInput, PensionContributionLineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionContributionLineService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionContributionLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionLineDto>({
      method: 'POST',
      url: '/api/erp/pension-contribution-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-contribution-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionLineDto>({
      method: 'GET',
      url: `/api/erp/pension-contribution-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionContributionLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionContributionLineDto>>({
      method: 'GET',
      url: '/api/erp/pension-contribution-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionContributionLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionContributionLineDto>({
      method: 'PUT',
      url: `/api/erp/pension-contribution-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
