import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { PayCalculationMethod } from './pay-calculation-method.enum';
import type { PayItemType } from './pay-item-type.enum';
import type { PayrollRunStatus } from './payroll-run-status.enum';
import type { PayslipLineType } from './payslip-line-type.enum';

export interface PayrollSetupDto {
  payrollRunNos?: string;
  personalRelief: number;
}

export interface PayrollEarningDto extends CodeTableDto {
  calculationMethod: PayCalculationMethod;
  defaultValue: number;
  basicPay: boolean;
  taxable: boolean;
  glAccountNo?: string;
  blocked: boolean;
}

export interface CreateUpdatePayrollEarningDto extends CreateUpdateCodeTableDto {
  calculationMethod: PayCalculationMethod;
  defaultValue: number;
  basicPay: boolean;
  taxable: boolean;
  glAccountNo?: string;
  blocked: boolean;
}

export interface PayrollDeductionDto extends CodeTableDto {
  calculationMethod: PayCalculationMethod;
  defaultValue: number;
  maximumAmount: number;
  taxDeductible: boolean;
  employerContributionPct: number;
  glAccountNo?: string;
  employerExpenseAccountNo?: string;
  statutory: boolean;
  blocked: boolean;
}

export interface CreateUpdatePayrollDeductionDto extends CreateUpdateCodeTableDto {
  calculationMethod: PayCalculationMethod;
  defaultValue: number;
  maximumAmount: number;
  taxDeductible: boolean;
  employerContributionPct: number;
  glAccountNo?: string;
  employerExpenseAccountNo?: string;
  statutory: boolean;
  blocked: boolean;
}

export interface PayrollTaxBandDto extends FullAuditedEntityDto<string> {
  lowerLimit: number;
  upperLimit: number;
  ratePct: number;
}

export interface CreateUpdatePayrollTaxBandDto {
  lowerLimit: number;
  upperLimit: number;
  ratePct: number;
}

export interface GetPayrollTaxBandListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
}

export interface EmployeePayItemDto extends FullAuditedEntityDto<string> {
  employeeNo?: string;
  itemType: PayItemType;
  code?: string;
  amount: number;
  startDate?: string;
  endDate?: string;
}

export interface CreateUpdateEmployeePayItemDto {
  employeeNo: string;
  itemType: PayItemType;
  code: string;
  amount: number;
  startDate?: string;
  endDate?: string;
}

export interface GetEmployeePayItemListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  employeeNo?: string;
}

export interface PayrollRunDto extends FullAuditedEntityDto<string> {
  no?: string;
  payPeriod?: string;
  postingDate?: string;
  description?: string;
  status: PayrollRunStatus;
  postedDate?: string;
  postedBy?: string;
  noOfEmployees: number;
  totalGross: number;
  totalDeductions: number;
  totalNet: number;
  totalEmployerContributions: number;
  paymentVoucherNo?: string;
}

export interface CreateUpdatePayrollRunDto {
  no?: string;
  payPeriod?: string;
  postingDate?: string;
  description?: string;
}

export interface GetPayrollRunListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  status?: PayrollRunStatus;
}

export interface PayslipDto extends EntityDto<string> {
  payrollRunNo?: string;
  payPeriod?: string;
  employeeNo?: string;
  employeeName?: string;
  jobTitle?: string;
  bankAccountNo?: string;
  basicPay: number;
  grossPay: number;
  taxablePay: number;
  incomeTax: number;
  totalDeductions: number;
  netPay: number;
  employerContributions: number;
}

export interface GetPayslipListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  payrollRunNo?: string;
  employeeNo?: string;
}

export interface PayslipLineDto extends EntityDto<string> {
  payrollRunNo?: string;
  employeeNo?: string;
  lineType: PayslipLineType;
  code?: string;
  description?: string;
  amount: number;
}
