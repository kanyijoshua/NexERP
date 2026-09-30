import type { CreateUpdateVatPostingSetupDto, VatPostingSetupDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class VatPostingSetupService {
  apiName = 'Erp';


  create = (input: CreateUpdateVatPostingSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VatPostingSetupDto>({
      method: 'POST',
      url: '/api/erp/vat-posting-setup',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/vat-posting-setup/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VatPostingSetupDto>({
      method: 'GET',
      url: `/api/erp/vat-posting-setup/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<VatPostingSetupDto>>({
      method: 'GET',
      url: '/api/erp/vat-posting-setup',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateVatPostingSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VatPostingSetupDto>({
      method: 'PUT',
      url: `/api/erp/vat-posting-setup/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
