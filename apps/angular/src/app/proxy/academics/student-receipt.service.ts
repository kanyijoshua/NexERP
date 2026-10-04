import type { CreateUpdateStudentReceiptDto, GetStudentDocumentListInput, StudentReceiptDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentReceiptService {
  apiName = 'Erp';


  create = (input: CreateUpdateStudentReceiptDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentReceiptDto>({
      method: 'POST',
      url: '/api/erp/student-receipt',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/student-receipt/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentReceiptDto>({
      method: 'GET',
      url: `/api/erp/student-receipt/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetStudentDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentReceiptDto>>({
      method: 'GET',
      url: '/api/erp/student-receipt',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, studentNo: input.studentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateStudentReceiptDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentReceiptDto>({
      method: 'PUT',
      url: `/api/erp/student-receipt/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentReceiptDto>({
      method: 'POST',
      url: `/api/erp/student-receipt/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
