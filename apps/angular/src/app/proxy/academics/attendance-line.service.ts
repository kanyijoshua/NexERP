import type { AttendanceLineDto, CreateUpdateAttendanceLineDto, GetDocumentLineListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AttendanceLineService {
  apiName = 'Erp';


  create = (input: CreateUpdateAttendanceLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceLineDto>({
      method: 'POST',
      url: '/api/erp/attendance-line',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/attendance-line/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceLineDto>({
      method: 'GET',
      url: `/api/erp/attendance-line/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetDocumentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AttendanceLineDto>>({
      method: 'GET',
      url: '/api/erp/attendance-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateAttendanceLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AttendanceLineDto>({
      method: 'PUT',
      url: `/api/erp/attendance-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
