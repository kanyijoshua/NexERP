import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetProjectLedgerListInput, ResourceLedgerEntryDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class ResourceLedgerEntryService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ResourceLedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/resource-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetProjectLedgerListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ResourceLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/resource-ledger-entry',
      params: { projectNo: input.projectNo, resourceNo: input.resourceNo, dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
