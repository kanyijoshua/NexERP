import type { CreateUpdateGradingBandDto, GetExamCategoryTableListInput, GradingBandDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class GradingBandService {
  apiName = 'Erp';


  create = (input: CreateUpdateGradingBandDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GradingBandDto>({
      method: 'POST',
      url: '/api/erp/grading-band',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/grading-band/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GradingBandDto>({
      method: 'GET',
      url: `/api/erp/grading-band/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetExamCategoryTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<GradingBandDto>>({
      method: 'GET',
      url: '/api/erp/grading-band',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, examCategoryCode: input.examCategoryCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateGradingBandDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GradingBandDto>({
      method: 'PUT',
      url: `/api/erp/grading-band/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
