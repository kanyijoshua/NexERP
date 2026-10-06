import type { CreateUpdatePensionerPayModeDto, PensionerPayModeDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionerPayModeService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionerPayModeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerPayModeDto>({
      method: 'POST',
      url: '/api/erp/pensioner-pay-mode',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pensioner-pay-mode/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerPayModeDto>({
      method: 'GET',
      url: `/api/erp/pensioner-pay-mode/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionerPayModeDto>>({
      method: 'GET',
      url: '/api/erp/pensioner-pay-mode',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionerPayModeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerPayModeDto>({
      method: 'PUT',
      url: `/api/erp/pensioner-pay-mode/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
