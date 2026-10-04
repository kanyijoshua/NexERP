import type { CreateUpdateUserSetupDto, GetUserSetupListInput, UserSetupDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class UserSetupService {
  apiName = 'Erp';


  create = (input: CreateUpdateUserSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, UserSetupDto>({
      method: 'POST',
      url: '/api/erp/user-setup',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/user-setup/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, UserSetupDto>({
      method: 'GET',
      url: `/api/erp/user-setup/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetUserSetupListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<UserSetupDto>>({
      method: 'GET',
      url: '/api/erp/user-setup',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateUserSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, UserSetupDto>({
      method: 'PUT',
      url: `/api/erp/user-setup/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
