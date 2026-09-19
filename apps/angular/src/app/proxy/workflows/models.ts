import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ApprovalDocumentKind } from './approval-document-kind.enum';
import type { ApprovalStatus } from './approval-status.enum';
import type { ApproverLimitType } from './approver-limit-type.enum';

export interface ApprovalCommentInput {
  comment?: string;
}

export interface ApprovalEntryDto extends EntityDto<string> {
  documentKind: ApprovalDocumentKind;
  documentId?: string;
  documentNo?: string;
  workflowCode?: string;
  sequenceNo: number;
  senderId?: string;
  senderUserName?: string;
  approverId?: string;
  approverUserName?: string;
  status: ApprovalStatus;
  amount: number;
  dueDate?: string;
  comment?: string;
  creationTime?: string;
  lastStatusChangeTime?: string;
  canAct: boolean;
}

export interface ApprovalUserSetupDto extends EntityDto<string> {
  userId?: string;
  userName?: string;
  approverUserId?: string;
  approverUserName?: string;
  substituteUserId?: string;
  substituteUserName?: string;
  salesAmountApprovalLimit: number;
  unlimitedSalesApproval: boolean;
  purchaseAmountApprovalLimit: number;
  unlimitedPurchaseApproval: boolean;
  isApprovalAdministrator: boolean;
}

export interface CreateUpdateApprovalUserSetupDto {
  userId?: string;
  userName: string;
  approverUserId?: string;
  substituteUserId?: string;
  salesAmountApprovalLimit: number;
  unlimitedSalesApproval: boolean;
  purchaseAmountApprovalLimit: number;
  unlimitedPurchaseApproval: boolean;
  isApprovalAdministrator: boolean;
}

export interface CreateUpdateWorkflowDto {
  code: string;
  description?: string;
  documentKind: ApprovalDocumentKind;
  minimumAmount: number;
  approverLimitType: ApproverLimitType;
  dueDays: number;
}

export interface GetApprovalEntriesInput extends PagedAndSortedResultRequestDto {
  status?: ApprovalStatus;
  allStatuses: boolean;
  onlyMine: boolean;
  sentByMe: boolean;
  documentId?: string;
}

export interface WorkflowDto extends EntityDto<string> {
  code?: string;
  description?: string;
  category?: string;
  enabled: boolean;
  documentKind: ApprovalDocumentKind;
  minimumAmount: number;
  approverLimitType: ApproverLimitType;
  dueDays: number;
  steps: WorkflowStepDto[];
}

export interface WorkflowStepDto extends EntityDto<string> {
  sequenceNo: number;
  eventName?: string;
  conditionRule?: string;
  responseAction?: string;
}

export interface ApprovalRequestResultDto {
  autoApproved: boolean;
  approverCount: number;
  firstApproverUserName?: string;
}
