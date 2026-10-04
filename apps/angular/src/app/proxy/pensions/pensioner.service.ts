import type { CreateUpdatePensionerDto, GetPensionerListInput, PensionerDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionerService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerDto>({
      method: 'POST',
      url: '/api/erp/pensioner',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pensioner/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerDto>({
      method: 'GET',
      url: `/api/erp/pensioner/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionerListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionerDto>>({
      method: 'GET',
      url: '/api/erp/pensioner',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, schemeCode: input.schemeCode, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerDto>({
      method: 'PUT',
      url: `/api/erp/pensioner/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
