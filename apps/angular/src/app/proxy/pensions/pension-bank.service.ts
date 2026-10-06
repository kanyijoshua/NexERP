import type { CreateUpdatePensionBankDto, PensionBankDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionBankService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionBankDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBankDto>({
      method: 'POST',
      url: '/api/erp/pension-bank',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-bank/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBankDto>({
      method: 'GET',
      url: `/api/erp/pension-bank/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionBankDto>>({
      method: 'GET',
      url: '/api/erp/pension-bank',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionBankDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionBankDto>({
      method: 'PUT',
      url: `/api/erp/pension-bank/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
