import type { ClinicVisitDto, CreateUpdateClinicVisitDto, GetCampusDocumentListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ClinicVisitService {
  apiName = 'Erp';


  create = (input: CreateUpdateClinicVisitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ClinicVisitDto>({
      method: 'POST',
      url: '/api/erp/clinic-visit',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/clinic-visit/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ClinicVisitDto>({
      method: 'GET',
      url: `/api/erp/clinic-visit/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCampusDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ClinicVisitDto>>({
      method: 'GET',
      url: '/api/erp/clinic-visit',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateClinicVisitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ClinicVisitDto>({
      method: 'PUT',
      url: `/api/erp/clinic-visit/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  complete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ClinicVisitDto>({
      method: 'POST',
      url: `/api/erp/clinic-visit/${id}/complete`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
