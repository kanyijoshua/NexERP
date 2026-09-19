import type { GetKanbanStagesInput, KanbanStageDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class KanbanPipelineService {
  apiName = 'Erp';
  

  getStages = (input: GetKanbanStagesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<KanbanStageDto>>({
      method: 'GET',
      url: '/api/erp/kanban-pipeline/stages',
      params: { pipelineType: input.pipelineType },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
