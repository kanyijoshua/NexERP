import type { CreateUpdatePensionerPayItemAssignmentDto, GetPensionerPayItemAssignmentListInput, PensionerPayItemAssignmentDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PensionerPayItemAssignmentService {
  apiName = 'Erp';


  create = (input: CreateUpdatePensionerPayItemAssignmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerPayItemAssignmentDto>({
      method: 'POST',
      url: '/api/erp/pensioner-pay-item-assignment',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pensioner-pay-item-assignment/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerPayItemAssignmentDto>({
      method: 'GET',
      url: `/api/erp/pensioner-pay-item-assignment/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetPensionerPayItemAssignmentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PensionerPayItemAssignmentDto>>({
      method: 'GET',
      url: '/api/erp/pensioner-pay-item-assignment',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, pensionerNo: input.pensionerNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdatePensionerPayItemAssignmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PensionerPayItemAssignmentDto>({
      method: 'PUT',
      url: `/api/erp/pensioner-pay-item-assignment/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
