import type { CreateUpdateShortCourseDto, ShortCourseDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShortCourseService {
  apiName = 'Erp';


  create = (input: CreateUpdateShortCourseDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseDto>({
      method: 'POST',
      url: '/api/erp/short-course',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/short-course/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseDto>({
      method: 'GET',
      url: `/api/erp/short-course/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShortCourseDto>>({
      method: 'GET',
      url: '/api/erp/short-course',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdateShortCourseDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShortCourseDto>({
      method: 'PUT',
      url: `/api/erp/short-course/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
