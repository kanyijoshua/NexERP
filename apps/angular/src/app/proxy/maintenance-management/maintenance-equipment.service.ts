import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateMaintenanceEquipmentDto, GetMaintenanceListInput, MaintenanceEquipmentDto } from './maintenance.models';

@Injectable({
  providedIn: 'root',
})
export class MaintenanceEquipmentService {
  apiName = 'Erp';

  create = (input: CreateUpdateMaintenanceEquipmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceEquipmentDto>({
      method: 'POST',
      url: '/api/erp/maintenance-equipment',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/maintenance-equipment/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceEquipmentDto>({
      method: 'GET',
      url: `/api/erp/maintenance-equipment/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetMaintenanceListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MaintenanceEquipmentDto>>({
      method: 'GET',
      url: '/api/erp/maintenance-equipment',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, equipmentNo: input.equipmentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateMaintenanceEquipmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MaintenanceEquipmentDto>({
      method: 'PUT',
      url: `/api/erp/maintenance-equipment/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
