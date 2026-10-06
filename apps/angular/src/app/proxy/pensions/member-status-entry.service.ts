import type { GetMemberStatusEntryListInput, MemberStatusEntryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MemberStatusEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberStatusEntryDto>({
      method: 'GET',
      url: `/api/erp/member-status-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetMemberStatusEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MemberStatusEntryDto>>({
      method: 'GET',
      url: '/api/erp/member-status-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, memberNo: input.memberNo, schemeCode: input.schemeCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
