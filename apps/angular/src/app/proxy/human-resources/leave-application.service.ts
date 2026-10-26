import type { AccrueLeaveInput, AccrueLeaveResultDto, ApproveLeaveApplicationInput, CalculateLeaveDatesInput, CreateUpdateLeaveApplicationDto, GetLeaveApplicationListInput, GetLeaveBalancesInput, GetLeaveLedgerEntryListInput, LeaveApplicationDto, LeaveBalanceDto, LeaveLedgerEntryDto, RecallLeaveApplicationInput, RejectLeaveApplicationInput } from './leave-management.models';
import type { WorkingPeriodDto } from './base-calendar.models';
import type { ApprovalRequestResultDto } from '../workflows/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LeaveApplicationService {
  apiName = 'Erp';


  accrue = (input: AccrueLeaveInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, AccrueLeaveResultDto>({
      method: 'POST',
      url: '/api/erp/leave-application/accrue',
      body: input,
    },
    { apiName: this.apiName,...config });


  approve = (id: string, input: ApproveLeaveApplicationInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveApplicationDto>({
      method: 'POST',
      url: `/api/erp/leave-application/${id}/approve`,
      body: input,
    },
    { apiName: this.apiName,...config });


  calculateDates = (input: CalculateLeaveDatesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WorkingPeriodDto>({
      method: 'POST',
      url: '/api/erp/leave-application/calculate-dates',
      body: input,
    },
    { apiName: this.apiName,...config });


  cancelApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/erp/leave-application/${id}/cancel-approval-request`,
    },
    { apiName: this.apiName,...config });


  create = (input: CreateUpdateLeaveApplicationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveApplicationDto>({
      method: 'POST',
      url: '/api/erp/leave-application',
      body: input,
    },
    { apiName: this.apiName,...config });


  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/erp/leave-application/${id}`,
    },
    { apiName: this.apiName,...config });


  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveApplicationDto>({
      method: 'GET',
      url: `/api/erp/leave-application/${id}`,
    },
    { apiName: this.apiName,...config });


  getBalances = (input: GetLeaveBalancesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveBalanceDto[]>({
      method: 'GET',
      url: '/api/erp/leave-application/balances',
      params: { employeeId: input.employeeId, leaveTypeCode: input.leaveTypeCode, leavePeriodYear: input.leavePeriodYear },
    },
    { apiName: this.apiName,...config });


  getLedgerEntries = (input: GetLeaveLedgerEntryListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LeaveLedgerEntryDto>>({
      method: 'GET',
      url: '/api/erp/leave-application/ledger-entries',
      params: { dynamicFilter: input.dynamicFilter, employeeId: input.employeeId, employeeNo: input.employeeNo, leaveTypeCode: input.leaveTypeCode, leavePeriodYear: input.leavePeriodYear, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  getList = (input: GetLeaveApplicationListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LeaveApplicationDto>>({
      method: 'GET',
      url: '/api/erp/leave-application',
      params: { dynamicFilter: input.dynamicFilter, filter: input.filter, employeeId: input.employeeId, leaveTypeCode: input.leaveTypeCode, leaveStatus: input.leaveStatus, leavePeriodYear: input.leavePeriodYear, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });


  recall = (id: string, input: RecallLeaveApplicationInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveApplicationDto>({
      method: 'POST',
      url: `/api/erp/leave-application/${id}/recall`,
      body: input,
    },
    { apiName: this.apiName,...config });


  reject = (id: string, input: RejectLeaveApplicationInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveApplicationDto>({
      method: 'POST',
      url: `/api/erp/leave-application/${id}/reject`,
      body: input,
    },
    { apiName: this.apiName,...config });


  reopen = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveApplicationDto>({
      method: 'POST',
      url: `/api/erp/leave-application/${id}/reopen`,
    },
    { apiName: this.apiName,...config });


  runPosting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveApplicationDto>({
      method: 'POST',
      url: `/api/erp/leave-application/${id}/run-posting`,
    },
    { apiName: this.apiName,...config });


  sendApprovalRequest = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApprovalRequestResultDto>({
      method: 'POST',
      url: `/api/erp/leave-application/${id}/send-approval-request`,
    },
    { apiName: this.apiName,...config });


  update = (id: string, input: CreateUpdateLeaveApplicationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LeaveApplicationDto>({
      method: 'PUT',
      url: `/api/erp/leave-application/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
