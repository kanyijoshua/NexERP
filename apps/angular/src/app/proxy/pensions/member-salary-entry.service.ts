import type { GetMemberSalaryEntryListInput, MemberSalaryEntryDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MemberSalaryEntryService {
  apiName = 'Erp';


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberSalaryEntryDto>({
      method: 'GET',
      url: `/api/erp/member-salary-entry/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetMemberSalaryEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MemberSalaryEntryDto>>({
      method: 'GET',
      url: '/api/erp/member-salary-entry',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, memberNo: input.memberNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
