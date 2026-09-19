import type { EntityDto } from '@abp/ng.core';

export interface GetKanbanStagesInput {
  pipelineType: string;
}

export interface KanbanStageDto extends EntityDto<string> {
  pipelineType?: string;
  sequence: number;
  name?: string;
  foldedInKanban: boolean;
  isWonStage: boolean;
}
