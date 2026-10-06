import type { CreateUpdatePensionTaxReliefLimitDto, GetPensionTaxReliefLimitListInput, PensionTaxReliefLimitDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionTaxReliefLimitService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionTaxReliefLimitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionTaxReliefLimitDto>({
      method: 'POST',
      url: '/api/erp/pension-tax-relief-limit',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-tax-relief-limit/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionTaxReliefLimitDto>({
      method: 'GET',
      url: `/api/erp/pension-tax-relief-limit/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionTaxReliefLimitListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionTaxReliefLimitDto>>({
      method: 'GET',
      url: '/api/erp/pension-tax-relief-limit',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionTaxReliefLimitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionTaxReliefLimitDto>({
      method: 'PUT',
      url: `/api/erp/pension-tax-relief-limit/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
