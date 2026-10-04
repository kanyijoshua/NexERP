import type { CreateUpdateVendorBankAccountDto, GetVendorBankAccountListInput, VendorBankAccountDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class VendorBankAccountService {
  apiName = 'Erp';


  create = (input: CreateUpdateVendorBankAccountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VendorBankAccountDto>({
      method: 'POST',
      url: '/api/erp/vendor-bank-account',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/vendor-bank-account/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VendorBankAccountDto>({
      method: 'GET',
      url: `/api/erp/vendor-bank-account/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetVendorBankAccountListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<VendorBankAccountDto>>({
      method: 'GET',
      url: '/api/erp/vendor-bank-account',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, vendorNo: input.vendorNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateVendorBankAccountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VendorBankAccountDto>({
      method: 'PUT',
      url: `/api/erp/vendor-bank-account/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
