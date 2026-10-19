import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateFinanceChargeTermsDto, FinanceChargeTermsDto } from './finance-operations.models';

@Injectable({
  providedIn: 'root',
})
export class FinanceChargeTermsService {
  apiName = 'Erp';

  create = (input: CreateUpdateFinanceChargeTermsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FinanceChargeTermsDto>({
      method: 'POST',
      url: '/api/erp/finance-charge-terms',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/finance-charge-terms/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FinanceChargeTermsDto>({
      method: 'GET',
      url: `/api/erp/finance-charge-terms/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FinanceChargeTermsDto>>({
      method: 'GET',
      url: '/api/erp/finance-charge-terms',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateFinanceChargeTermsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FinanceChargeTermsDto>({
      method: 'PUT',
      url: `/api/erp/finance-charge-terms/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
