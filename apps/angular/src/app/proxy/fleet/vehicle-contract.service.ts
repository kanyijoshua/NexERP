import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateVehicleContractDto, GetFleetListInput, VehicleContractDto } from './fleet.models';

@Injectable({
  providedIn: 'root',
})
export class VehicleContractService {
  apiName = 'Erp';

  create = (input: CreateUpdateVehicleContractDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VehicleContractDto>({
      method: 'POST',
      url: '/api/erp/vehicle-contract',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/vehicle-contract/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VehicleContractDto>({
      method: 'GET',
      url: `/api/erp/vehicle-contract/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetFleetListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<VehicleContractDto>>({
      method: 'GET',
      url: '/api/erp/vehicle-contract',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, vehicleNo: input.vehicleNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateVehicleContractDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VehicleContractDto>({
      method: 'PUT',
      url: `/api/erp/vehicle-contract/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
