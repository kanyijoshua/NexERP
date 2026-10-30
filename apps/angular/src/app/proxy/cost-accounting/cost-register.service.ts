import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CostRegisterDto, ErpPagedListInput } from './cost-accounting.models';

@Injectable({
  providedIn: 'root',
})
export class CostRegisterService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostRegisterDto>({
      method: 'GET',
      url: `/api/erp/cost-register/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: ErpPagedListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CostRegisterDto>>({
      method: 'GET',
      url: '/api/erp/cost-register',
      params: { dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
