import type { CreateUpdatePensionSchemeDto, PensionSchemeDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionSchemeService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionSchemeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionSchemeDto>({
      method: 'POST',
      url: '/api/erp/pension-scheme',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-scheme/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionSchemeDto>({
      method: 'GET',
      url: `/api/erp/pension-scheme/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionSchemeDto>>({
      method: 'GET',
      url: '/api/erp/pension-scheme',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionSchemeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionSchemeDto>({
      method: 'PUT',
      url: `/api/erp/pension-scheme/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
