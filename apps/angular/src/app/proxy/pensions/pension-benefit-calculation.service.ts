import type { CreateUpdatePensionBenefitCalculationDto, GetPensionBenefitCalculationListInput, PensionBenefitCalculationDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionBenefitCalculationService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionBenefitCalculationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBenefitCalculationDto>({
      method: 'POST',
      url: '/api/erp/pension-benefit-calculation',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-benefit-calculation/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBenefitCalculationDto>({
      method: 'GET',
      url: `/api/erp/pension-benefit-calculation/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPensionBenefitCalculationListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionBenefitCalculationDto>>({
      method: 'GET',
      url: '/api/erp/pension-benefit-calculation',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, schemeCode: input.schemeCode, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePensionBenefitCalculationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBenefitCalculationDto>({
      method: 'PUT',
      url: `/api/erp/pension-benefit-calculation/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  calculate = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBenefitCalculationDto>({
      method: 'POST',
      url: `/api/erp/pension-benefit-calculation/${id}/calculate`,
    },
    { apiName: this.apiName,...config });

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBenefitCalculationDto>({
      method: 'POST',
      url: `/api/erp/pension-benefit-calculation/${id}/approve`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
