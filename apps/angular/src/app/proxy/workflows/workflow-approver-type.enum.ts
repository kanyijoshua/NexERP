import { mapEnumToOptions } from '@abp/ng.core';

export enum WorkflowApproverType {
  Approver = 0,
  WorkflowUserGroup = 1,
}

export const workflowApproverTypeOptions = mapEnumToOptions(WorkflowApproverType);
