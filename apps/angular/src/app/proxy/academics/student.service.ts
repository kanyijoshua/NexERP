import type { CreateUpdateStudentDto, GetStudentListInput, StudentBalanceDto, StudentDto, StudentTranscriptDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentService {
  apiName = 'Erp';


  create = (input: CreateUpdateStudentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentDto>({
      method: 'POST',
      url: '/api/erp/student',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/student/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentDto>({
      method: 'GET',
      url: `/api/erp/student/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetStudentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentDto>>({
      method: 'GET',
      url: '/api/erp/student',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, programmeCode: input.programmeCode, stageCode: input.stageCode, intakeCode: input.intakeCode, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateStudentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentDto>({
      method: 'PUT',
      url: `/api/erp/student/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  getBalance = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentBalanceDto>({
      method: 'GET',
      url: `/api/erp/student/${id}/balance`,
    },
    { apiName: this.apiName,...config });


  getTranscript = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentTranscriptDto>({
      method: 'GET',
      url: `/api/erp/student/${id}/transcript`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
