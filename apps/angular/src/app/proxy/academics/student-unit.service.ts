import type { CreateUpdateStudentUnitDto, GetStudentUnitListInput, StudentUnitDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentUnitService {
  apiName = 'Erp';


  create = (input: CreateUpdateStudentUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentUnitDto>({
      method: 'POST',
      url: '/api/erp/student-unit',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/student-unit/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentUnitDto>({
      method: 'GET',
      url: `/api/erp/student-unit/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetStudentUnitListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentUnitDto>>({
      method: 'GET',
      url: '/api/erp/student-unit',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, registrationNo: input.registrationNo, studentNo: input.studentNo, unitCode: input.unitCode, semesterCode: input.semesterCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateStudentUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentUnitDto>({
      method: 'PUT',
      url: `/api/erp/student-unit/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
