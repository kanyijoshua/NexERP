import type { CreateUpdateProgrammeDto, ProgrammeDto } from './models';
import type { GetCodeTableListInput } from '../companies/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ProgrammeService {
  apiName = 'Erp';


  create = (input: CreateUpdateProgrammeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProgrammeDto>({
      method: 'POST',
      url: '/api/erp/programme',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/programme/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProgrammeDto>({
      method: 'GET',
      url: `/api/erp/programme/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetCodeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProgrammeDto>>({
      method: 'GET',
      url: '/api/erp/programme',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateProgrammeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProgrammeDto>({
      method: 'PUT',
      url: `/api/erp/programme/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
