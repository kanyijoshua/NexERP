import type { CauseOfAbsenceDto, CreateUpdateCauseOfAbsenceDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CauseOfAbsenceService {
  apiName = 'Erp';


  create = (input: CreateUpdateCauseOfAbsenceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CauseOfAbsenceDto>({
      method: 'POST',
      url: '/api/erp/cause-of-absence',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/cause-of-absence/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CauseOfAbsenceDto>({
      method: 'GET',
      url: `/api/erp/cause-of-absence/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CauseOfAbsenceDto>>({
      method: 'GET',
      url: '/api/erp/cause-of-absence',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateCauseOfAbsenceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CauseOfAbsenceDto>({
      method: 'PUT',
      url: `/api/erp/cause-of-absence/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
