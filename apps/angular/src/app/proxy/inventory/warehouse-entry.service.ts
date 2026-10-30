import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { GetWarehouseEntryListInput, WarehouseEntryDto } from './warehouse-management.models';

@Injectable({
  providedIn: 'root',
})
export class WarehouseEntryService {
  apiName = 'Erp';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseEntryDto>({
      method: 'GET',
      url: `/api/erp/warehouse-entry/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetWarehouseEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WarehouseEntryDto>>({
      method: 'GET',
      url: '/api/erp/warehouse-entry',
      params: { dynamicFilter: input.dynamicFilter, locationCode: input.locationCode, binCode: input.binCode, itemNo: input.itemNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
