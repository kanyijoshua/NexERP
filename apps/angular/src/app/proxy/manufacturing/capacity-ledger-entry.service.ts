import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CapacityLedgerEntryDto, GetCapacityLedgerListInput } from './manufacturing.models';

@Injectable({
  providedIn: 'root',
})
export class CapacityLedgerEntryService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CapacityLedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/capacity-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCapacityLedgerListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CapacityLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/capacity-ledger-entry',
      params: { productionOrderNo: input.productionOrderNo, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
