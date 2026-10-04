import type { ConfidentialInformationDto, CreateUpdateConfidentialInformationDto, GetConfidentialInformationListInput } from './base-tables.models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ConfidentialInformationService {
  apiName = 'Erp';


  create = (input: CreateUpdateConfidentialInformationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfidentialInformationDto>({
      method: 'POST',
      url: '/api/erp/confidential-information',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/confidential-information/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfidentialInformationDto>({
      method: 'GET',
      url: `/api/erp/confidential-information/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetConfidentialInformationListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ConfidentialInformationDto>>({
      method: 'GET',
      url: '/api/erp/confidential-information',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, employeeNo: input.employeeNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateConfidentialInformationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ConfidentialInformationDto>({
      method: 'PUT',
      url: `/api/erp/confidential-information/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
