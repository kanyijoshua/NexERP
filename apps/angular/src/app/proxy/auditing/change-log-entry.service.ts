import type { ChangeLogEntryDto, GetChangeLogEntryListInput, PurgeChangeLogEntriesInput, PurgeChangeLogEntriesResultDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ChangeLogEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ChangeLogEntryDto>({
      method: 'GET',
      url: `/api/erp/change-log-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetChangeLogEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ChangeLogEntryDto>>({
      method: 'GET',
      url: '/api/erp/change-log-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, entityName: input.entityName, recordId: input.recordId, fieldName: input.fieldName, typeOfChange: input.typeOfChange, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  purgeEntries = (input: PurgeChangeLogEntriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurgeChangeLogEntriesResultDto>({
      method: 'POST',
      url: '/api/erp/change-log-entry/purge-entries',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
