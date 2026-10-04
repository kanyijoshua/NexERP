import type { CreateUpdateExamCategoryDto, ExamCategoryDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExamCategoryService {
  apiName = 'Erp';


  create = (input: CreateUpdateExamCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamCategoryDto>({
      method: 'POST',
      url: '/api/erp/exam-category',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/exam-category/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamCategoryDto>({
      method: 'GET',
      url: `/api/erp/exam-category/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ExamCategoryDto>>({
      method: 'GET',
      url: '/api/erp/exam-category',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateExamCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamCategoryDto>({
      method: 'PUT',
      url: `/api/erp/exam-category/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
