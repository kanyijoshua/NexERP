import type { CreateUpdatePensionerPayItemDto, PensionerPayItemDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionerPayItemService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionerPayItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerPayItemDto>({
      method: 'POST',
      url: '/api/erp/pensioner-pay-item',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pensioner-pay-item/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerPayItemDto>({
      method: 'GET',
      url: `/api/erp/pensioner-pay-item/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionerPayItemDto>>({
      method: 'GET',
      url: '/api/erp/pensioner-pay-item',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionerPayItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerPayItemDto>({
      method: 'PUT',
      url: `/api/erp/pensioner-pay-item/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
