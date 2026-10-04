import type { CreateUpdateExamComponentDto, ExamComponentDto, GetExamCategoryTableListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExamComponentService {
  apiName = 'Erp';


  create = (input: CreateUpdateExamComponentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamComponentDto>({
      method: 'POST',
      url: '/api/erp/exam-component',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/exam-component/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamComponentDto>({
      method: 'GET',
      url: `/api/erp/exam-component/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetExamCategoryTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ExamComponentDto>>({
      method: 'GET',
      url: '/api/erp/exam-component',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, examCategoryCode: input.examCategoryCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateExamComponentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamComponentDto>({
      method: 'PUT',
      url: `/api/erp/exam-component/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
