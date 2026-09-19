import type { CreateUpdateWorkflowDto, WorkflowDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WorkflowService {
  apiName = 'Erp';
  

  create = (input: CreateUpdateWorkflowDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkflowDto>({
      method: 'POST',
      url: '/api/erp/workflow',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/workflow/${id}`,
    },
    { apiName: this.apiName,...config });
  

  disable = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/workflow/${id}/disable`,
    },
    { apiName: this.apiName,...config });
  

  enable = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/workflow/${id}/enable`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkflowDto>({
      method: 'GET',
      url: `/api/erp/workflow/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<WorkflowDto>>({
      method: 'GET',
      url: '/api/erp/workflow',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateWorkflowDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkflowDto>({
      method: 'PUT',
      url: `/api/erp/workflow/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
