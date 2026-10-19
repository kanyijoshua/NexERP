import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetProjectLedgerListInput, ProjectLedgerEntryDto } from './projects.models';

@Injectable({
  providedIn: 'root',
})
export class ProjectLedgerEntryService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProjectLedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/project-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetProjectLedgerListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProjectLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/project-ledger-entry',
      params: { projectNo: input.projectNo, dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
