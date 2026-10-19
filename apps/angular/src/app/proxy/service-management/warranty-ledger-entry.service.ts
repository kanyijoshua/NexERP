import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetServiceLedgerListInput, WarrantyLedgerEntryDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class WarrantyLedgerEntryService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarrantyLedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/warranty-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetServiceLedgerListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WarrantyLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/warranty-ledger-entry',
      params: { serviceOrderNo: input.serviceOrderNo, serviceItemNo: input.serviceItemNo, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
