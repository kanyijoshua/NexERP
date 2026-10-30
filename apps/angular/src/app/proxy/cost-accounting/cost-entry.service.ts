import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CostEntryDto, GetCostEntryListInput } from './cost-accounting.models';

@Injectable({
  providedIn: 'root',
})
export class CostEntryService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostEntryDto>({
      method: 'GET',
      url: `/api/erp/cost-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCostEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CostEntryDto>>({
      method: 'GET',
      url: '/api/erp/cost-entry',
      params: { dynamicFilter: input.dynamicFilter, costTypeNo: input.costTypeNo, costCenterCode: input.costCenterCode, costObjectCode: input.costObjectCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
