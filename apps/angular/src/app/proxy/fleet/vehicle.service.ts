import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateVehicleDto, GetFleetListInput, GetVehicleCostsInput, VehicleCostDto, VehicleDto } from './fleet.models';

@Injectable({
  providedIn: 'root',
})
export class VehicleService {
  apiName = 'Erp';

  create = (input: CreateUpdateVehicleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VehicleDto>({
      method: 'POST',
      url: '/api/erp/vehicle',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/vehicle/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VehicleDto>({
      method: 'GET',
      url: `/api/erp/vehicle/${id}`,
    },
    { apiName: this.apiName,...config });

  getCosts = (input: GetVehicleCostsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VehicleCostDto[]>({
      method: 'GET',
      url: '/api/erp/vehicle/costs',
      params: { fromDate: input.fromDate, toDate: input.toDate },
    },
    { apiName: this.apiName,...config });

  getList = (input: GetFleetListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<VehicleDto>>({
      method: 'GET',
      url: '/api/erp/vehicle',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, vehicleNo: input.vehicleNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateVehicleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VehicleDto>({
      method: 'PUT',
      url: `/api/erp/vehicle/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
