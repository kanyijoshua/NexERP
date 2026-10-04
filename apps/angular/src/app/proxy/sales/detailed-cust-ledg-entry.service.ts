import type { DetailedCustLedgEntryDto, GetDetailedCustLedgEntryListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DetailedCustLedgEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DetailedCustLedgEntryDto>({
      method: 'GET',
      url: `/api/erp/detailed-cust-ledg-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetDetailedCustLedgEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<DetailedCustLedgEntryDto>>({
      method: 'GET',
      url: '/api/erp/detailed-cust-ledg-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, customerNo: input.customerNo, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
