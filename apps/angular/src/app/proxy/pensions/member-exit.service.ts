import type { CreateUpdateMemberExitDto, GetMemberExitListInput, MemberExitDto, PostMemberExitInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MemberExitService {
  apiName = 'Erp';


  create = (input: CreateUpdateMemberExitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDto>({
      method: 'POST',
      url: '/api/erp/member-exit',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/member-exit/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDto>({
      method: 'GET',
      url: `/api/erp/member-exit/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetMemberExitListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MemberExitDto>>({
      method: 'GET',
      url: '/api/erp/member-exit',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, memberNo: input.memberNo, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateMemberExitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDto>({
      method: 'PUT',
      url: `/api/erp/member-exit/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDto>({
      method: 'POST',
      url: `/api/erp/member-exit/${id}/approve`,
    },
    { apiName: this.apiName,...config });


  calculate = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDto>({
      method: 'POST',
      url: `/api/erp/member-exit/${id}/calculate`,
    },
    { apiName: this.apiName,...config });


  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDto>({
      method: 'POST',
      url: `/api/erp/member-exit/${id}/reopen`,
    },
    { apiName: this.apiName,...config });


  runPosting = (id: string, input: PostMemberExitInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDto>({
      method: 'POST',
      url: `/api/erp/member-exit/${id}/run-posting`,
      body: input,
    },
    { apiName: this.apiName,...config });


  raisePaymentVoucher = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MemberExitDto>({
      method: 'POST',
      url: `/api/erp/member-exit/${id}/raise-payment-voucher`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
