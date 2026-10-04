import type { AttendanceRegisterDto, CreateUpdateAttendanceRegisterDto, GetCampusDocumentListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AttendanceRegisterService {
  apiName = 'Erp';


  create = (input: CreateUpdateAttendanceRegisterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceRegisterDto>({
      method: 'POST',
      url: '/api/erp/attendance-register',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/attendance-register/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceRegisterDto>({
      method: 'GET',
      url: `/api/erp/attendance-register/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCampusDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AttendanceRegisterDto>>({
      method: 'GET',
      url: '/api/erp/attendance-register',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateAttendanceRegisterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceRegisterDto>({
      method: 'PUT',
      url: `/api/erp/attendance-register/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  suggestLines = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceRegisterDto>({
      method: 'POST',
      url: `/api/erp/attendance-register/${id}/suggest-lines`,
    },
    { apiName: this.apiName,...config });

  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceRegisterDto>({
      method: 'POST',
      url: `/api/erp/attendance-register/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
