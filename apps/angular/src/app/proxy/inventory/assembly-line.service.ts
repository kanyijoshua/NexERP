import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { AssemblyLineDto, GetAssemblyLineListInput } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class AssemblyLineService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AssemblyLineDto>({
      method: 'GET',
      url: `/api/erp/assembly-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetAssemblyLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AssemblyLineDto>>({
      method: 'GET',
      url: '/api/erp/assembly-line',
      params: { dynamicFilter: input.dynamicFilter, assemblyHeaderId: input.assemblyHeaderId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
