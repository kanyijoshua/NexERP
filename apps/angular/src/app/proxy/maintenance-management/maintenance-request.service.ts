import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateMaintenanceRequestDto, GeneratePreventiveInput, GeneratePreventiveResultDto, GetMaintenanceListInput, MaintenanceRequestDto } from './maintenance.models';

@Injectable({
  providedIn: 'root',
})
export class MaintenanceRequestService {
  apiName = 'Erp';

  complete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceRequestDto>({
      method: 'POST',
      url: `/api/erp/maintenance-request/${id}/complete`,
    },
    { apiName: this.apiName,...config });

  create = (input: CreateUpdateMaintenanceRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceRequestDto>({
      method: 'POST',
      url: '/api/erp/maintenance-request',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/maintenance-request/${id}`,
    },
    { apiName: this.apiName,...config });

  generatePreventive = (input: GeneratePreventiveInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GeneratePreventiveResultDto>({
      method: 'POST',
      url: '/api/erp/maintenance-request/generate-preventive',
      body: input,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceRequestDto>({
      method: 'GET',
      url: `/api/erp/maintenance-request/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetMaintenanceListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MaintenanceRequestDto>>({
      method: 'GET',
      url: '/api/erp/maintenance-request',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, equipmentNo: input.equipmentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  scrap = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceRequestDto>({
      method: 'POST',
      url: `/api/erp/maintenance-request/${id}/scrap`,
    },
    { apiName: this.apiName,...config });

  start = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceRequestDto>({
      method: 'POST',
      url: `/api/erp/maintenance-request/${id}/start`,
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateMaintenanceRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceRequestDto>({
      method: 'PUT',
      url: `/api/erp/maintenance-request/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
