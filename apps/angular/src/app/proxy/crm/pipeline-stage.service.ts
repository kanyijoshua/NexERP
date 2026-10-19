import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdatePipelineStageDto, GetPipelineStageListInput, PipelineStageSetupDto } from './crm.models';

@Injectable({
  providedIn: 'root',
})
export class PipelineStageService {
  apiName = 'Erp';

  create = (input: CreateUpdatePipelineStageDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PipelineStageSetupDto>({
      method: 'POST',
      url: '/api/erp/pipeline-stage',
      body: input,
    },
    { apiName: this.apiName,...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/pipeline-stage/${id}`,
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PipelineStageSetupDto>({
      method: 'GET',
      url: `/api/erp/pipeline-stage/${id}`,
    },
    { apiName: this.apiName,...config });

  getList = (input: GetPipelineStageListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PipelineStageSetupDto>>({
      method: 'GET',
      url: '/api/erp/pipeline-stage',
      params: { pipelineType: input.pipelineType, dynamicFilter: input.dynamicFilter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  update = (id: string, input: CreateUpdatePipelineStageDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PipelineStageSetupDto>({
      method: 'PUT',
      url: `/api/erp/pipeline-stage/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
