import type { CreateUpdateShortCourseParticipantDto, GetDocumentLineListInput, ShortCourseParticipantDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShortCourseParticipantService {
  apiName = 'Erp';


  create = (input: CreateUpdateShortCourseParticipantDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseParticipantDto>({
      method: 'POST',
      url: '/api/erp/short-course-participant',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/short-course-participant/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseParticipantDto>({
      method: 'GET',
      url: `/api/erp/short-course-participant/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetDocumentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShortCourseParticipantDto>>({
      method: 'GET',
      url: '/api/erp/short-course-participant',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateShortCourseParticipantDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseParticipantDto>({
      method: 'PUT',
      url: `/api/erp/short-course-participant/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
