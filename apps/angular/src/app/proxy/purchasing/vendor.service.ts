import type { CreateUpdateVendorDto, GetVendorListInput, VendorDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class vendorService {
  apiName = 'Erp';
  

  block = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/vendor/${id}/block`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateVendorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VendorDto>({
      method: 'POST',
      url: '/api/erp/vendor',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/vendor/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VendorDto>({
      method: 'GET',
      url: `/api/erp/vendor/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getByNo = (no: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VendorDto>({
      method: 'GET',
      url: '/api/erp/vendor/by-no',
      params: { no },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetVendorListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<VendorDto>>({
      method: 'GET',
      url: '/api/erp/vendor',
      params: { filter: input.filter, blocked: input.blocked, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  unblock = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/vendor/${id}/unblock`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateVendorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VendorDto>({
      method: 'PUT',
      url: `/api/erp/vendor/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
