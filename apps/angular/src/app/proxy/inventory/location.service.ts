import type { CreateUpdateLocationDto, LocationDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LocationService {
  apiName = 'Erp';


  create = (input: CreateUpdateLocationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LocationDto>({
      method: 'POST',
      url: '/api/erp/location',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/location/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LocationDto>({
      method: 'GET',
      url: `/api/erp/location/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LocationDto>>({
      method: 'GET',
      url: '/api/erp/location',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateLocationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LocationDto>({
      method: 'PUT',
      url: `/api/erp/location/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
