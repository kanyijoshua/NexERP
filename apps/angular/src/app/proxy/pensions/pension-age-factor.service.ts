import type { CreateUpdatePensionAgeFactorDto, GetPensionAgeFactorListInput, PensionAgeFactorDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionAgeFactorService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionAgeFactorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionAgeFactorDto>({
      method: 'POST',
      url: '/api/erp/pension-age-factor',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-age-factor/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionAgeFactorDto>({
      method: 'GET',
      url: `/api/erp/pension-age-factor/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionAgeFactorListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionAgeFactorDto>>({
      method: 'GET',
      url: '/api/erp/pension-age-factor',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, schemeCode: input.schemeCode, factorType: input.factorType, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionAgeFactorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionAgeFactorDto>({
      method: 'PUT',
      url: `/api/erp/pension-age-factor/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
