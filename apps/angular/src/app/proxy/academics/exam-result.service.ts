import type { CreateUpdateExamResultHeaderDto, ExamResultHeaderDto, GetExamResultListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExamResultService {
  apiName = 'Erp';


  create = (input: CreateUpdateExamResultHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamResultHeaderDto>({
      method: 'POST',
      url: '/api/erp/exam-result',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/exam-result/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamResultHeaderDto>({
      method: 'GET',
      url: `/api/erp/exam-result/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetExamResultListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ExamResultHeaderDto>>({
      method: 'GET',
      url: '/api/erp/exam-result',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, programmeCode: input.programmeCode, unitCode: input.unitCode, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateExamResultHeaderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamResultHeaderDto>({
      method: 'PUT',
      url: `/api/erp/exam-result/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });


  suggestLines = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamResultHeaderDto>({
      method: 'POST',
      url: `/api/erp/exam-result/${id}/suggest-lines`,
    },
    { apiName: this.apiName,...config });


  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamResultHeaderDto>({
      method: 'POST',
      url: `/api/erp/exam-result/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
