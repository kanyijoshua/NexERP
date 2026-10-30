import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetCodeTableListInput } from '../companies/models';
import type { CreateUpdateMaintenanceTeamDto, MaintenanceTeamDto } from './maintenance.models';

@Injectable({
  providedIn: 'root',
})
export class MaintenanceTeamService {
  apiName = 'Erp';

  create = (input: CreateUpdateMaintenanceTeamDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceTeamDto>({
      method: 'POST',
      url: '/api/erp/maintenance-team',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/maintenance-team/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceTeamDto>({
      method: 'GET',
      url: `/api/erp/maintenance-team/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MaintenanceTeamDto>>({
      method: 'GET',
      url: '/api/erp/maintenance-team',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateMaintenanceTeamDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceTeamDto>({
      method: 'PUT',
      url: `/api/erp/maintenance-team/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
