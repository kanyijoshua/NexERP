import type { AcademicYearDto, CreateUpdateAcademicYearDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AcademicYearService {
  apiName = 'Erp';


  create = (input: CreateUpdateAcademicYearDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AcademicYearDto>({
      method: 'POST',
      url: '/api/erp/academic-year',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/academic-year/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AcademicYearDto>({
      method: 'GET',
      url: `/api/erp/academic-year/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<AcademicYearDto>>({
      method: 'GET',
      url: '/api/erp/academic-year',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateAcademicYearDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AcademicYearDto>({
      method: 'PUT',
      url: `/api/erp/academic-year/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
