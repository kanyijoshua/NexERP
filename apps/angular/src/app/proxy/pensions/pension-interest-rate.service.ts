import type { AllocateInterestInput, CreateUpdatePensionInterestRateDto, GetPensionInterestRateListInput, InterestAllocationDto, PensionInterestRateDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionInterestRateService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionInterestRateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionInterestRateDto>({
      method: 'POST',
      url: '/api/erp/pension-interest-rate',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-interest-rate/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionInterestRateDto>({
      method: 'GET',
      url: `/api/erp/pension-interest-rate/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionInterestRateListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionInterestRateDto>>({
      method: 'GET',
      url: '/api/erp/pension-interest-rate',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, schemeCode: input.schemeCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionInterestRateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionInterestRateDto>({
      method: 'PUT',
      url: `/api/erp/pension-interest-rate/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  allocate = (id: string, input: AllocateInterestInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionInterestRateDto>({
      method: 'POST',
      url: `/api/erp/pension-interest-rate/${id}/allocate`,
      body: input,
    },
    { apiName: this.apiName,...config });


  getPreview = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InterestAllocationDto>({
      method: 'GET',
      url: `/api/erp/pension-interest-rate/${id}/preview`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
