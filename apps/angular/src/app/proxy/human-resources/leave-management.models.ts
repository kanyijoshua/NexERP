import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { EmployeeGender } from './employee-gender.enum';
import type { LeaveAccrualFrequency, LeaveApplicationStatus, LeaveEntryType } from './leave-enums';

export interface LeaveTypeDto extends CodeTableDto {
  causeOfAbsenceCode?: string;
  defaultEntitlementDays: number;
  maxCarryForwardDays: number;
  accrualFrequency: LeaveAccrualFrequency;
  paid: boolean;
  requiresApproval: boolean;
  genderRestriction?: EmployeeGender;
}

export interface CreateUpdateLeaveTypeDto extends CreateUpdateCodeTableDto {
  causeOfAbsenceCode?: string;
  defaultEntitlementDays: number;
  maxCarryForwardDays: number;
  accrualFrequency: LeaveAccrualFrequency;
  paid: boolean;
  requiresApproval: boolean;
  genderRestriction?: EmployeeGender;
}

export interface LeaveLedgerEntryDto {
  id?: string;
  entryNo: number;
  employeeId?: string;
  employeeNo?: string;
  employeeName?: string;
  leaveTypeCode?: string;
  leavePeriodYear: number;
  postingDate?: string;
  entryType: LeaveEntryType;
  documentNo?: string;
  fromDate?: string;
  toDate?: string;
  days: number;
  description?: string;
}

export interface GetLeaveLedgerEntryListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  employeeId?: string;
  employeeNo?: string;
  leaveTypeCode?: string;
  leavePeriodYear?: number;
  filter?: string;
}

export interface LeaveBalanceDto {
  employeeId?: string;
  employeeNo?: string;
  employeeName?: string;
  leaveTypeCode?: string;
  leaveTypeDescription?: string;
  leavePeriodYear: number;
  entitledDays: number;
  accruedDays: number;
  carriedForwardDays: number;
  takenDays: number;
  recalledDays: number;
  adjustedDays: number;
  balanceDays: number;
}

export interface GetLeaveBalancesInput {
  employeeId?: string;
  leaveTypeCode?: string;
  leavePeriodYear?: number;
}

export interface AccrueLeaveInput {
  leaveTypeCode: string;
  leavePeriodYear: number;
  postingDate?: string;
  employeeId?: string;
  daysOverride?: number;
  carryForwardFromPreviousYear: boolean;
}

export interface AccrueLeaveResultDto {
  employeesProcessed: number;
}

export interface LeaveApplicationDto extends FullAuditedEntityDto<string> {
  no?: string;
  employeeId?: string;
  employeeNo?: string;
  employeeName?: string;
  leaveTypeCode?: string;
  leavePeriodYear: number;
  applicationDate?: string;
  startDate?: string;
  endDate?: string;
  returnDate?: string;
  daysApplied: number;
  daysApproved: number;
  balanceBefore: number;
  reason?: string;
  relieverEmployeeNo?: string;
  approverComment?: string;
  leaveStatus: LeaveApplicationStatus;
  postedAbsenceId?: string;
  recalledDays: number;
  recallDate?: string;
  recallReason?: string;
}

export interface GetLeaveApplicationListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  employeeId?: string;
  leaveTypeCode?: string;
  leaveStatus?: LeaveApplicationStatus;
  leavePeriodYear?: number;
}

export interface CreateUpdateLeaveApplicationDto {
  no?: string;
  employeeId: string;
  leaveTypeCode: string;
  applicationDate?: string;
  startDate: string;
  endDate?: string;
  returnDate?: string;
  daysApplied: number;
  reason?: string;
  relieverEmployeeNo?: string;
}

export interface ApproveLeaveApplicationInput {
  daysApproved?: number;
  comment?: string;
}

export interface RejectLeaveApplicationInput {
  comment?: string;
}

export interface RecallLeaveApplicationInput {
  recallDays: number;
  recallDate?: string;
  reason?: string;
}

export interface CalculateLeaveDatesInput {
  employeeId: string;
  startDate: string;
  daysApplied: number;
}
