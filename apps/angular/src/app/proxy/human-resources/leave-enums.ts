import { mapEnumToOptions } from '@abp/ng.core';

export enum LeaveAccrualFrequency {
  Annual = 0,
  Monthly = 1,
  Manual = 2,
}

export const leaveAccrualFrequencyOptions = mapEnumToOptions(LeaveAccrualFrequency);

export enum LeaveEntryType {
  Entitlement = 0,
  Accrual = 1,
  CarryOver = 2,
  Usage = 3,
  Recall = 4,
  Adjustment = 5,
}

export const leaveEntryTypeOptions = mapEnumToOptions(LeaveEntryType);

export enum LeaveApplicationStatus {
  Open = 0,
  PendingApproval = 1,
  Released = 2,
  Rejected = 3,
  Posted = 4,
  Recalled = 5,
}

export const leaveApplicationStatusOptions = mapEnumToOptions(LeaveApplicationStatus);
