import type { CreateUpdateMaintenanceRegistrationDto, GetMaintenanceRegistrationListInput, MaintenanceRegistrationDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MaintenanceRegistrationService {
  apiName = 'Erp';


  create = (input: CreateUpdateMaintenanceRegistrationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceRegistrationDto>({
      method: 'POST',
      url: '/api/erp/maintenance-registration',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/maintenance-registration/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceRegistrationDto>({
      method: 'GET',
      url: `/api/erp/maintenance-registration/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetMaintenanceRegistrationListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MaintenanceRegistrationDto>>({
      method: 'GET',
      url: '/api/erp/maintenance-registration',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, faNo: input.faNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateMaintenanceRegistrationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceRegistrationDto>({
      method: 'PUT',
      url: `/api/erp/maintenance-registration/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
