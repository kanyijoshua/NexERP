import type { AgedAccountsInput, FinancialReportPeriodInput, ReportExportInput, ReportResultDto, RunAccountScheduleInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FinancialReportService {
  apiName = 'Erp';
  

  getAgedAccounts = (input: AgedAccountsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportResultDto>({
      method: 'GET',
      url: '/api/erp/financial-report/aged-accounts',
      params: { kind: input.kind, asOfDate: input.asOfDate, agingMethod: input.agingMethod, periodLengthDays: input.periodLengthDays, excludeZeroBalances: input.excludeZeroBalances },
    },
    { apiName: this.apiName,...config });
  

  getBalanceSheet = (input: FinancialReportPeriodInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportResultDto>({
      method: 'GET',
      url: '/api/erp/financial-report/balance-sheet',
      params: { fromDate: input.fromDate, toDate: input.toDate, accountFilter: input.accountFilter, excludeZeroBalances: input.excludeZeroBalances },
    },
    { apiName: this.apiName,...config });
  

  getGLDetail = (input: FinancialReportPeriodInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportResultDto>({
      method: 'GET',
      url: '/api/erp/financial-report/g-lDetail',
      params: { fromDate: input.fromDate, toDate: input.toDate, accountFilter: input.accountFilter, excludeZeroBalances: input.excludeZeroBalances },
    },
    { apiName: this.apiName,...config });
  

  getIncomeStatement = (input: FinancialReportPeriodInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportResultDto>({
      method: 'GET',
      url: '/api/erp/financial-report/income-statement',
      params: { fromDate: input.fromDate, toDate: input.toDate, accountFilter: input.accountFilter, excludeZeroBalances: input.excludeZeroBalances },
    },
    { apiName: this.apiName,...config });
  

  getTrialBalance = (input: FinancialReportPeriodInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportResultDto>({
      method: 'GET',
      url: '/api/erp/financial-report/trial-balance',
      params: { fromDate: input.fromDate, toDate: input.toDate, accountFilter: input.accountFilter, excludeZeroBalances: input.excludeZeroBalances },
    },
    { apiName: this.apiName,...config });
  

  runExport = (input: ReportExportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/erp/financial-report/run-export',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  runSchedule = (input: RunAccountScheduleInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReportResultDto>({
      method: 'POST',
      url: '/api/erp/financial-report/run-schedule',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
