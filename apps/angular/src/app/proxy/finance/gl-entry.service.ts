import type { GLEntryDto, GetGLEntryListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class gl-entryService {
  apiName = 'Erp';
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GLEntryDto>({
      method: 'GET',
      url: `/api/erp/gl-entry/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getByDocument = (documentNo: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<GLEntryDto>>({
      method: 'GET',
      url: '/api/erp/gl-entry/by-document',
      params: { documentNo },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetGLEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<GLEntryDto>>({
      method: 'GET',
      url: '/api/erp/gl-entry',
      params: { glAccountId: input.glAccountId, documentNo: input.documentNo, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
