import type { CreateUpdateProgrammeStageDto, GetProgrammeTableListInput, ProgrammeStageDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ProgrammeStageService {
  apiName = 'Erp';


  create = (input: CreateUpdateProgrammeStageDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProgrammeStageDto>({
      method: 'POST',
      url: '/api/erp/programme-stage',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/programme-stage/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProgrammeStageDto>({
      method: 'GET',
      url: `/api/erp/programme-stage/${id}`,
    },
    { apiName: this.apiName,...config });


  getList = (input: GetProgrammeTableListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProgrammeStageDto>>({
      method: 'GET',
      url: '/api/erp/programme-stage',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, programmeCode: input.programmeCode, stageCode: input.stageCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateProgrammeStageDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProgrammeStageDto>({
      method: 'PUT',
      url: `/api/erp/programme-stage/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
