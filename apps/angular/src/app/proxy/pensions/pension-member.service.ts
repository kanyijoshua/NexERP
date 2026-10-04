import type { CreateUpdatePensionMemberDto, GetPensionMemberListInput, MemberBalanceDto, PensionMemberDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionMemberService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionMemberDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionMemberDto>({
      method: 'POST',
      url: '/api/erp/pension-member',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pension-member/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionMemberDto>({
      method: 'GET',
      url: `/api/erp/pension-member/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionMemberListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionMemberDto>>({
      method: 'GET',
      url: '/api/erp/pension-member',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, schemeCode: input.schemeCode, sponsorNo: input.sponsorNo, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionMemberDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionMemberDto>({
      method: 'PUT',
      url: `/api/erp/pension-member/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  getBalance = (id: string, asOfDate?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberBalanceDto>({
      method: 'GET',
      url: `/api/erp/pension-member/${id}/balance`,
      params: { asOfDate },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
