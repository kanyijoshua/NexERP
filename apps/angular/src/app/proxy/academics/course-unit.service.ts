import type { CourseUnitDto, CreateUpdateCourseUnitDto, GetProgrammeTableListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CourseUnitService {
  apiName = 'Erp';


  create = (input: CreateUpdateCourseUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CourseUnitDto>({
      method: 'POST',
      url: '/api/erp/course-unit',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/course-unit/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CourseUnitDto>({
      method: 'GET',
      url: `/api/erp/course-unit/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetProgrammeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CourseUnitDto>>({
      method: 'GET',
      url: '/api/erp/course-unit',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, programmeCode: input.programmeCode, stageCode: input.stageCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateCourseUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CourseUnitDto>({
      method: 'PUT',
      url: `/api/erp/course-unit/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
