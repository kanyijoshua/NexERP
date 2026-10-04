import type { DetailedVendorLedgEntryDto, GetDetailedVendorLedgEntryListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DetailedVendorLedgEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DetailedVendorLedgEntryDto>({
      method: 'GET',
      url: `/api/erp/detailed-vendor-ledg-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetDetailedVendorLedgEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<DetailedVendorLedgEntryDto>>({
      method: 'GET',
      url: '/api/erp/detailed-vendor-ledg-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, vendorNo: input.vendorNo, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
