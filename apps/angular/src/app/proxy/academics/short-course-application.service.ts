import type { CreateUpdateShortCourseApplicationDto, GetCampusDocumentListInput, RejectShortCourseApplicationInput, ShortCourseApplicationDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShortCourseApplicationService {
  apiName = 'Erp';


  create = (input: CreateUpdateShortCourseApplicationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseApplicationDto>({
      method: 'POST',
      url: '/api/erp/short-course-application',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/short-course-application/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseApplicationDto>({
      method: 'GET',
      url: `/api/erp/short-course-application/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCampusDocumentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShortCourseApplicationDto>>({
      method: 'GET',
      url: '/api/erp/short-course-application',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateShortCourseApplicationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseApplicationDto>({
      method: 'PUT',
      url: `/api/erp/short-course-application/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  submit = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseApplicationDto>({
      method: 'POST',
      url: `/api/erp/short-course-application/${id}/submit`,
    },
    { apiName: this.apiName,...config });

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseApplicationDto>({
      method: 'POST',
      url: `/api/erp/short-course-application/${id}/approve`,
    },
    { apiName: this.apiName,...config });

  reject = (id: string, input: RejectShortCourseApplicationInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseApplicationDto>({
      method: 'POST',
      url: `/api/erp/short-course-application/${id}/reject`,
      body: input,
    },
    { apiName: this.apiName,...config });

  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseApplicationDto>({
      method: 'POST',
      url: `/api/erp/short-course-application/${id}/reopen`,
    },
    { apiName: this.apiName,...config });

  register = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseApplicationDto>({
      method: 'POST',
      url: `/api/erp/short-course-application/${id}/register`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
