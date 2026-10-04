import type { CreateUpdateStudentStatusChangeDto, GetStudentDocumentListInput, StudentStatusChangeDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentStatusChangeService {
  apiName = 'Erp';


  create = (input: CreateUpdateStudentStatusChangeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentStatusChangeDto>({
      method: 'POST',
      url: '/api/erp/student-status-change',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/student-status-change/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentStatusChangeDto>({
      method: 'GET',
      url: `/api/erp/student-status-change/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetStudentDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentStatusChangeDto>>({
      method: 'GET',
      url: '/api/erp/student-status-change',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, studentNo: input.studentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateStudentStatusChangeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentStatusChangeDto>({
      method: 'PUT',
      url: `/api/erp/student-status-change/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentStatusChangeDto>({
      method: 'POST',
      url: `/api/erp/student-status-change/${id}/approve`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
