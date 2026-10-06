import type { CreateUpdateOtherPensionSchemeDto, OtherPensionSchemeDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class OtherPensionSchemeService {
  apiName = 'Erp';


  create = (input: CreateUpdateOtherPensionSchemeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OtherPensionSchemeDto>({
      method: 'POST',
      url: '/api/erp/other-pension-scheme',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/other-pension-scheme/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OtherPensionSchemeDto>({
      method: 'GET',
      url: `/api/erp/other-pension-scheme/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<OtherPensionSchemeDto>>({
      method: 'GET',
      url: '/api/erp/other-pension-scheme',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateOtherPensionSchemeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OtherPensionSchemeDto>({
      method: 'PUT',
      url: `/api/erp/other-pension-scheme/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
