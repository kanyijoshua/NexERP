import type { GetPensionerChangeEntryListInput, PensionerChangeEntryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionerChangeEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerChangeEntryDto>({
      method: 'GET',
      url: `/api/erp/pensioner-change-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionerChangeEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionerChangeEntryDto>>({
      method: 'GET',
      url: '/api/erp/pensioner-change-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, pensionerNo: input.pensionerNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
