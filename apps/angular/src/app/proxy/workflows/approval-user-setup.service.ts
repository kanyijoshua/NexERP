import type { ApprovalUserSetupDto, CreateUpdateApprovalUserSetupDto, GetApprovalUserSetupListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ApprovalUserSetupService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateApprovalUserSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalUserSetupDto>({
      method: 'POST',
      url: '/api/erp/approval-user-setup',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/approval-user-setup/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalUserSetupDto>({
      method: 'GET',
      url: `/api/erp/approval-user-setup/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetApprovalUserSetupListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ApprovalUserSetupDto>>({
      method: 'GET',
      url: '/api/erp/approval-user-setup',
      params: { filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount, dynamicFilter: input.dynamicFilter },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateApprovalUserSetupDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalUserSetupDto>({
      method: 'PUT',
      url: `/api/erp/approval-user-setup/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
