import type { AdmitApplicationInput, CreateUpdateStudentApplicationDto, GetStudentApplicationListInput, RejectApplicationInput, StudentApplicationDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentApplicationService {
  apiName = 'Erp';


  create = (input: CreateUpdateStudentApplicationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentApplicationDto>({
      method: 'POST',
      url: '/api/erp/student-application',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/student-application/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentApplicationDto>({
      method: 'GET',
      url: `/api/erp/student-application/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetStudentApplicationListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentApplicationDto>>({
      method: 'GET',
      url: '/api/erp/student-application',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, programmeCode: input.programmeCode, intakeCode: input.intakeCode, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateStudentApplicationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentApplicationDto>({
      method: 'PUT',
      url: `/api/erp/student-application/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  submit = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentApplicationDto>({
      method: 'POST',
      url: `/api/erp/student-application/${id}/submit`,
    },
    { apiName: this.apiName,...config });


  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentApplicationDto>({
      method: 'POST',
      url: `/api/erp/student-application/${id}/approve`,
    },
    { apiName: this.apiName,...config });


  reject = (id: string, input: RejectApplicationInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentApplicationDto>({
      method: 'POST',
      url: `/api/erp/student-application/${id}/reject`,
      body: input,
    },
    { apiName: this.apiName,...config });


  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentApplicationDto>({
      method: 'POST',
      url: `/api/erp/student-application/${id}/reopen`,
    },
    { apiName: this.apiName,...config });


  admit = (id: string, input: AdmitApplicationInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentApplicationDto>({
      method: 'POST',
      url: `/api/erp/student-application/${id}/admit`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
