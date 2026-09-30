import type { CreateUpdateInventoryPostingSetupDto, InventoryPostingSetupDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InventoryPostingSetupService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateInventoryPostingSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryPostingSetupDto>({
      method: 'POST',
      url: '/api/erp/inventory-posting-setup',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/inventory-posting-setup/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryPostingSetupDto>({
      method: 'GET',
      url: `/api/erp/inventory-posting-setup/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryPostingSetupDto>>({
      method: 'GET',
      url: '/api/erp/inventory-posting-setup',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateInventoryPostingSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryPostingSetupDto>({
      method: 'PUT',
      url: `/api/erp/inventory-posting-setup/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
