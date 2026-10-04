import type { CreateUpdateStudentBillLineDto, GetDocumentLineListInput, StudentBillLineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentBillLineService {
  apiName = 'Erp';


  create = (input: CreateUpdateStudentBillLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentBillLineDto>({
      method: 'POST',
      url: '/api/erp/student-bill-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/student-bill-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentBillLineDto>({
      method: 'GET',
      url: `/api/erp/student-bill-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetDocumentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentBillLineDto>>({
      method: 'GET',
      url: '/api/erp/student-bill-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateStudentBillLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentBillLineDto>({
      method: 'PUT',
      url: `/api/erp/student-bill-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
