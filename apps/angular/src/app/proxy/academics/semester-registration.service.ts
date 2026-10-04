import type { CreateUpdateSemesterRegistrationDto, GetSemesterRegistrationListInput, SemesterRegistrationDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SemesterRegistrationService {
  apiName = 'Erp';


  create = (input: CreateUpdateSemesterRegistrationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SemesterRegistrationDto>({
      method: 'POST',
      url: '/api/erp/semester-registration',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/semester-registration/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SemesterRegistrationDto>({
      method: 'GET',
      url: `/api/erp/semester-registration/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetSemesterRegistrationListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SemesterRegistrationDto>>({
      method: 'GET',
      url: '/api/erp/semester-registration',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, studentNo: input.studentNo, semesterCode: input.semesterCode, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateSemesterRegistrationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SemesterRegistrationDto>({
      method: 'PUT',
      url: `/api/erp/semester-registration/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  fillUnits = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SemesterRegistrationDto>({
      method: 'POST',
      url: `/api/erp/semester-registration/${id}/fill-units`,
    },
    { apiName: this.apiName,...config });


  submit = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SemesterRegistrationDto>({
      method: 'POST',
      url: `/api/erp/semester-registration/${id}/submit`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
