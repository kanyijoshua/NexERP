import type { CreateUpdateLeaveTypeDto, LeaveTypeDto } from './leave-management.models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LeaveTypeService {
  apiName = 'Erp';


  create = (input: CreateUpdateLeaveTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveTypeDto>({
      method: 'POST',
      url: '/api/erp/leave-type',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/leave-type/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveTypeDto>({
      method: 'GET',
      url: `/api/erp/leave-type/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LeaveTypeDto>>({
      method: 'GET',
      url: '/api/erp/leave-type',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateLeaveTypeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveTypeDto>({
      method: 'PUT',
      url: `/api/erp/leave-type/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
