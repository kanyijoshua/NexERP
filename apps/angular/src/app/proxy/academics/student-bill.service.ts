import type { CreateUpdateStudentBillHeaderDto, GetStudentBillListInput, StudentBillHeaderDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentBillService {
  apiName = 'Erp';


  create = (input: CreateUpdateStudentBillHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentBillHeaderDto>({
      method: 'POST',
      url: '/api/erp/student-bill',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/student-bill/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentBillHeaderDto>({
      method: 'GET',
      url: `/api/erp/student-bill/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetStudentBillListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentBillHeaderDto>>({
      method: 'GET',
      url: '/api/erp/student-bill',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, studentNo: input.studentNo, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateStudentBillHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentBillHeaderDto>({
      method: 'PUT',
      url: `/api/erp/student-bill/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  suggestLines = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentBillHeaderDto>({
      method: 'POST',
      url: `/api/erp/student-bill/${id}/suggest-lines`,
    },
    { apiName: this.apiName,...config });


  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentBillHeaderDto>({
      method: 'POST',
      url: `/api/erp/student-bill/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
