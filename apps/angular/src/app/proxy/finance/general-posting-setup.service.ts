import type { CreateUpdateGeneralPostingSetupDto, GeneralPostingSetupDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class GeneralPostingSetupService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateGeneralPostingSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GeneralPostingSetupDto>({
      method: 'POST',
      url: '/api/erp/general-posting-setup',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/general-posting-setup/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GeneralPostingSetupDto>({
      method: 'GET',
      url: `/api/erp/general-posting-setup/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<GeneralPostingSetupDto>>({
      method: 'GET',
      url: '/api/erp/general-posting-setup',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateGeneralPostingSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GeneralPostingSetupDto>({
      method: 'PUT',
      url: `/api/erp/general-posting-setup/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
