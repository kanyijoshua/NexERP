import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateStandardVendorPurchaseCodeDto, GetStandardVendorPurchaseCodeListInput, StandardVendorPurchaseCodeDto } from './standard-codes.models';

@Injectable({
  providedIn: 'root',
})
export class StandardVendorPurchaseCodeService {
  apiName = 'Erp';

  create = (input: CreateUpdateStandardVendorPurchaseCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardVendorPurchaseCodeDto>({
      method: 'POST',
      url: '/api/erp/standard-vendor-purchase-code',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/standard-vendor-purchase-code/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardVendorPurchaseCodeDto>({
      method: 'GET',
      url: `/api/erp/standard-vendor-purchase-code/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetStandardVendorPurchaseCodeListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StandardVendorPurchaseCodeDto>>({
      method: 'GET',
      url: '/api/erp/standard-vendor-purchase-code',
      params: { dynamicFilter: input.dynamicFilter, vendorNo: input.vendorNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateStandardVendorPurchaseCodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StandardVendorPurchaseCodeDto>({
      method: 'PUT',
      url: `/api/erp/standard-vendor-purchase-code/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
