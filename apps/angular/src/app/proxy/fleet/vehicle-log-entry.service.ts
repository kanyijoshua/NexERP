import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateVehicleLogEntryDto, GetFleetListInput, VehicleLogEntryDto } from './fleet.models';

@Injectable({
  providedIn: 'root',
})
export class VehicleLogEntryService {
  apiName = 'Erp';

  create = (input: CreateVehicleLogEntryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VehicleLogEntryDto>({
      method: 'POST',
      url: '/api/erp/vehicle-log-entry',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/vehicle-log-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VehicleLogEntryDto>({
      method: 'GET',
      url: `/api/erp/vehicle-log-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetFleetListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<VehicleLogEntryDto>>({
      method: 'GET',
      url: '/api/erp/vehicle-log-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, vehicleNo: input.vehicleNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
