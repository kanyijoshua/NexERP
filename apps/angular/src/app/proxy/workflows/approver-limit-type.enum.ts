import { mapEnumToOptions } from '@abp/ng.core';

export enum ApproverLimitType {
  ApproverChain = 0,
  DirectApprover = 1,
  FirstQualifiedApprover = 2,
}

export const approverLimitTypeOptions = mapEnumToOptions(ApproverLimitType);
