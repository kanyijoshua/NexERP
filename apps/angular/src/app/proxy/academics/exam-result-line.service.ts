import type { CreateUpdateExamResultLineDto, ExamResultLineDto, GetDocumentLineListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExamResultLineService {
  apiName = 'Erp';


  create = (input: CreateUpdateExamResultLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamResultLineDto>({
      method: 'POST',
      url: '/api/erp/exam-result-line',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/exam-result-line/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamResultLineDto>({
      method: 'GET',
      url: `/api/erp/exam-result-line/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetDocumentLineListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ExamResultLineDto>>({
      method: 'GET',
      url: '/api/erp/exam-result-line',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, documentNo: input.documentNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateExamResultLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExamResultLineDto>({
      method: 'PUT',
      url: `/api/erp/exam-result-line/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
