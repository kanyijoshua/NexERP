import type { CreateUpdateWorkflowUserGroupMemberDto, GetWorkflowUserGroupMemberListInput, WorkflowUserGroupMemberDto } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WorkflowUserGroupMemberService {
  apiName = 'Erp';


  create = (input: CreateUpdateWorkflowUserGroupMemberDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkflowUserGroupMemberDto>({
      method: 'POST',
      url: '/api/erp/workflow-user-group-member',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/workflow-user-group-member/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkflowUserGroupMemberDto>({
      method: 'GET',
      url: `/api/erp/workflow-user-group-member/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetWorkflowUserGroupMemberListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WorkflowUserGroupMemberDto>>({
      method: 'GET',
      url: '/api/erp/workflow-user-group-member',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, workflowUserGroupCode: input.workflowUserGroupCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateWorkflowUserGroupMemberDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkflowUserGroupMemberDto>({
      method: 'PUT',
      url: `/api/erp/workflow-user-group-member/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
