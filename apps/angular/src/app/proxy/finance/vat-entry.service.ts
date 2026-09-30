import type { GetVatEntryListInput, VatEntryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class VatEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, VatEntryDto>({
      method: 'GET',
      url: `/api/erp/vat-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetVatEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<VatEntryDto>>({
      method: 'GET',
      url: '/api/erp/vat-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, type: input.type, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
