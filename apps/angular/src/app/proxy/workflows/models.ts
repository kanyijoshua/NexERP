import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface ApprovalEntryDto extends EntityDto<string> {
  tableName?: string;
  documentId?: string;
  documentNo?: string;
  senderId?: string;
  approverId?: string;
  status?: string;
  amount: number;
  creationTime?: string;
}

export interface GetApprovalEntriesInput extends PagedAndSortedResultRequestDto {
  status?: string;
  onlyMine: boolean;
}

export interface WorkflowDto extends EntityDto<string> {
  code?: string;
  description?: string;
  category?: string;
  enabled: boolean;
  steps: WorkflowStepDto[];
}

export interface WorkflowStepDto extends EntityDto<string> {
  sequenceNo: number;
  eventName?: string;
  conditionRule?: string;
  responseAction?: string;
}
