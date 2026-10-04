import type { GetMemberLedgerEntryListInput, MemberLedgerEntryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MemberLedgerEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberLedgerEntryDto>({
      method: 'GET',
      url: `/api/erp/member-ledger-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetMemberLedgerEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MemberLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/member-ledger-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, memberNo: input.memberNo, schemeCode: input.schemeCode, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
