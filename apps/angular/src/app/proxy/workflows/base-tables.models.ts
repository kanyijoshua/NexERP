import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ApprovalDocumentType } from './approval-document-type.enum';

export interface WorkflowUserGroupMemberDto extends FullAuditedEntityDto<string> {
  workflowUserGroupCode?: string;
  userName?: string;
  sequenceNo: number;
}

export interface CreateUpdateWorkflowUserGroupMemberDto {
  workflowUserGroupCode: string;
  userName: string;
  sequenceNo: number;
}

export interface GetWorkflowUserGroupMemberListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  workflowUserGroupCode?: string;
}

export interface ApprovalCommentLineDto extends FullAuditedEntityDto<string> {
  entryNo: number;
  tableId: number;
  documentType: ApprovalDocumentType;
  documentNo?: string;
  userId?: string;
  dateAndTime?: string;
  comment?: string;
}

export interface CreateUpdateApprovalCommentLineDto {
  entryNo: number;
  tableId: number;
  documentType: ApprovalDocumentType;
  documentNo?: string;
  userId?: string;
  dateAndTime?: string;
  comment?: string;
}

export interface GetApprovalCommentLineListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  documentNo?: string;
}
