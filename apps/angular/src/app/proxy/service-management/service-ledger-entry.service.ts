import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetServiceLedgerListInput, ServiceLedgerEntryDto } from './service-management.models';

@Injectable({
  providedIn: 'root',
})
export class ServiceLedgerEntryService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ServiceLedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/service-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetServiceLedgerListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ServiceLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/service-ledger-entry',
      params: { serviceOrderNo: input.serviceOrderNo, serviceItemNo: input.serviceItemNo, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
