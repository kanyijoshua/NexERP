import { Injectable, inject } from '@angular/core';
import {
  CreateUpdateEmployeePayItemDto,
  CreateUpdatePayrollRunDto,
  CreateUpdatePayrollTaxBandDto,
  EmployeePayItemDto,
  EmployeePayItemService,
  PayCalculationMethod,
  PayItemType,
  PayrollDeductionService,
  PayrollEarningService,
  PayrollRunDto,
  PayrollRunService,
  PayrollRunStatus,
  PayrollTaxBandDto,
  PayrollTaxBandService,
  PayslipDto,
  PayslipLineDto,
  PayslipLineService,
  PayslipLineType,
  PayslipService,
  payCalculationMethodOptions,
  payItemTypeOptions,
  payrollRunStatusOptions,
  payslipLineTypeOptions,
} from '@proxy/payroll';
import { EMPTY, map } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { accountField, codeField, codeTableEntity, enumOptions } from './entity-helpers';

const PERMISSION = 'Erp.Payroll';
const SETUP_PERMISSION = 'Erp.PayrollSetup';
const today = () => new Date().toISOString().substring(0, 10);
const firstOfMonth = () => `${today().substring(0, 8)}01`;

function figure(field: string, labelKey: string, section = 'general'): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

/**
 * Payroll: earnings, deductions and the tax bands they are worked out with, each employee's
 * recurring pay items, and the monthly runs with their payslips. Joined into `MasterDataEntities.all`.
 */
@Injectable({ providedIn: 'root' })
export class PayrollEntities {
  private readonly earnings = inject(PayrollEarningService);
  private readonly deductions = inject(PayrollDeductionService);
  private readonly taxBands = inject(PayrollTaxBandService);
  private readonly payItems = inject(EmployeePayItemService);
  private readonly runs = inject(PayrollRunService);
  private readonly payslips = inject(PayslipService);
  private readonly payslipLines = inject(PayslipLineService);

  readonly payrollEarning = codeTableEntity(this.earnings, {
    key: 'payrollEarning',
    titleKey: 'Erp::PayrollEarning',
    pluralKey: 'Erp::PayrollEarnings',
    icon: 'fas fa-money-bill-wave',
    permission: SETUP_PERMISSION,
    route: '/erp/payroll-earnings',
    columns: [
      { field: 'calculationMethod', labelKey: 'Erp::CalculationMethod', type: 'select', options: enumOptions(payCalculationMethodOptions, 'PayCalculationMethod') },
      { field: 'defaultValue', labelKey: 'Erp::DefaultValue', type: 'number' },
      { field: 'basicPay', labelKey: 'Erp::BasicPay', type: 'boolean' },
      { field: 'taxable', labelKey: 'Erp::Taxable', type: 'boolean' },
      { field: 'glAccountNo', labelKey: 'Erp::GLAccountNo' },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean' },
    ],
    fields: [
      {
        field: 'calculationMethod',
        labelKey: 'Erp::CalculationMethod',
        type: 'select',
        options: enumOptions(payCalculationMethodOptions, 'PayCalculationMethod').filter(o => (o.value as number) <= PayCalculationMethod.PercentOfBasic),
      },
      { field: 'defaultValue', labelKey: 'Erp::DefaultValue', type: 'number', min: 0, helpKey: 'Erp::PayDefaultValueHelp' },
      { field: 'basicPay', labelKey: 'Erp::BasicPay', type: 'checkbox', helpKey: 'Erp::BasicPayHelp' },
      { field: 'taxable', labelKey: 'Erp::Taxable', type: 'checkbox' },
      accountField('glAccountNo', 'Erp::GLAccountNo', 'general', true),
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    defaults: { calculationMethod: PayCalculationMethod.FlatAmount, defaultValue: 0, basicPay: false, taxable: true, blocked: false },
    quickCreate: false,
  });

  readonly payrollDeduction = codeTableEntity(this.deductions, {
    key: 'payrollDeduction',
    titleKey: 'Erp::PayrollDeduction',
    pluralKey: 'Erp::PayrollDeductions',
    icon: 'fas fa-scissors',
    permission: SETUP_PERMISSION,
    route: '/erp/payroll-deductions',
    columns: [
      { field: 'calculationMethod', labelKey: 'Erp::CalculationMethod', type: 'select', options: enumOptions(payCalculationMethodOptions, 'PayCalculationMethod') },
      { field: 'defaultValue', labelKey: 'Erp::DefaultValue', type: 'number' },
      { field: 'statutory', labelKey: 'Erp::Statutory', type: 'boolean' },
      { field: 'taxDeductible', labelKey: 'Erp::TaxDeductible', type: 'boolean' },
      { field: 'employerContributionPct', labelKey: 'Erp::EmployerContributionPct', type: 'number' },
      { field: 'glAccountNo', labelKey: 'Erp::GLAccountNo' },
    ],
    fields: [
      { field: 'calculationMethod', labelKey: 'Erp::CalculationMethod', type: 'select', options: enumOptions(payCalculationMethodOptions, 'PayCalculationMethod') },
      { field: 'defaultValue', labelKey: 'Erp::DefaultValue', type: 'number', min: 0, helpKey: 'Erp::PayDefaultValueHelp' },
      { field: 'maximumAmount', labelKey: 'Erp::MaximumAmount', type: 'currency', min: 0, helpKey: 'Erp::MaximumAmountHelp' },
      { field: 'statutory', labelKey: 'Erp::Statutory', type: 'checkbox', helpKey: 'Erp::StatutoryHelp' },
      { field: 'taxDeductible', labelKey: 'Erp::TaxDeductible', type: 'checkbox', helpKey: 'Erp::TaxDeductibleHelp' },
      { field: 'employerContributionPct', labelKey: 'Erp::EmployerContributionPct', type: 'number', min: 0, helpKey: 'Erp::EmployerContributionHelp' },
      accountField('glAccountNo', 'Erp::GLAccountNo', 'general', true),
      accountField('employerExpenseAccountNo', 'Erp::EmployerExpenseAccountNo', 'general'),
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    defaults: {
      calculationMethod: PayCalculationMethod.FlatAmount,
      defaultValue: 0,
      maximumAmount: 0,
      statutory: false,
      taxDeductible: false,
      employerContributionPct: 0,
      blocked: false,
    },
    quickCreate: false,
  });

  readonly payrollTaxBand: RecordEntity<PayrollTaxBandDto, CreateUpdatePayrollTaxBandDto> = {
    key: 'payrollTaxBand',
    titleKey: 'Erp::PayrollTaxBand',
    pluralKey: 'Erp::PayrollTaxBands',
    icon: 'fas fa-layer-group',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/payroll-tax-bands'],
    columns: [
      { field: 'lowerLimit', labelKey: 'Erp::LowerLimit', type: 'currency' },
      { field: 'upperLimit', labelKey: 'Erp::UpperLimit', type: 'currency' },
      { field: 'ratePct', labelKey: 'Erp::RatePct', type: 'number' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'lowerLimit', labelKey: 'Erp::LowerLimit', type: 'currency', min: 0 },
      { field: 'upperLimit', labelKey: 'Erp::UpperLimit', type: 'currency', min: 0, helpKey: 'Erp::TopBandHelp' },
      { field: 'ratePct', labelKey: 'Erp::RatePct', type: 'number', min: 0 },
    ],
    getList: query => this.taxBands.getList(query),
    get: id => this.taxBands.get(id),
    create: input => this.taxBands.create(input),
    update: (id, input) => this.taxBands.update(id, input),
    delete: id => this.taxBands.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.lowerLimit}`, name: `${dto.ratePct}%` }),
    newRecord: () => ({ lowerLimit: 0, upperLimit: 0, ratePct: 0 }),
  };

  readonly employeePayItem: RecordEntity<EmployeePayItemDto, CreateUpdateEmployeePayItemDto> = {
    key: 'employeePayItem',
    titleKey: 'Erp::EmployeePayItem',
    pluralKey: 'Erp::EmployeePayItems',
    icon: 'fas fa-user-tag',
    permission: PERMISSION,
    listRoute: ['/erp/employee-pay-items'],
    columns: [
      { field: 'employeeNo', labelKey: 'Erp::EmployeeNo' },
      { field: 'itemType', labelKey: 'Erp::ItemType', type: 'select', options: enumOptions(payItemTypeOptions, 'PayItemType') },
      { field: 'code', labelKey: 'Erp::Code' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('employeeNo', 'Erp::EmployeeNo', 'employee', undefined, { required: true }),
      { field: 'itemType', labelKey: 'Erp::ItemType', type: 'select', options: enumOptions(payItemTypeOptions, 'PayItemType') },
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true, maxLength: 20, helpKey: 'Erp::PayItemCodeHelp' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', min: 0, helpKey: 'Erp::PayItemAmountHelp' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
    ],
    getList: query => this.payItems.getList(query),
    get: id => this.payItems.get(id),
    create: input => this.payItems.create(input),
    update: (id, input) => this.payItems.update(id, input),
    delete: id => this.payItems.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.employeeNo ?? ''} ${dto.code ?? ''}`.trim(), name: PayItemType[dto.itemType] }),
    newRecord: () => ({ itemType: PayItemType.Earning, amount: 0 }),
  };

  readonly payrollRun: RecordEntity<PayrollRunDto, CreateUpdatePayrollRunDto> = {
    key: 'payrollRun',
    titleKey: 'Erp::PayrollRun',
    pluralKey: 'Erp::PayrollRuns',
    icon: 'fas fa-money-check-dollar',
    permission: PERMISSION,
    listRoute: ['/erp/payroll-runs'],
    attachmentEntityType: 'PayrollRun',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'payPeriod', labelKey: 'Erp::PayPeriod', type: 'date' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(payrollRunStatusOptions, 'PayrollRunStatus') },
      { field: 'noOfEmployees', labelKey: 'Erp::NoOfEmployees', type: 'number' },
      { field: 'totalGross', labelKey: 'Erp::TotalGross', type: 'currency' },
      { field: 'totalNet', labelKey: 'Erp::TotalNet', type: 'currency' },
      { field: 'paymentVoucherNo', labelKey: 'Erp::PaymentVoucherNo' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      { field: 'payPeriod', labelKey: 'Erp::PayPeriod', type: 'date', required: true, helpKey: 'Erp::PayPeriodHelp' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      figure('totalDeductions', 'Erp::TotalDeductions'),
      figure('totalEmployerContributions', 'Erp::TotalEmployerContributions'),
      figure('postedBy', 'Erp::PostedBy'),
    ],
    getList: query => this.runs.getList(query),
    get: id => this.runs.get(id),
    create: input => this.runs.create(input),
    update: (id, input) => this.runs.update(id, input),
    delete: id => this.runs.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.payPeriod?.substring(0, 7) }),
    newRecord: () => ({ payPeriod: firstOfMonth(), postingDate: today() }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: PayrollRunStatus[dto.status] },
      { labelKey: 'Erp::NoOfEmployees', value: dto.noOfEmployees, type: 'number' },
      { labelKey: 'Erp::TotalGross', value: dto.totalGross, type: 'currency' },
      { labelKey: 'Erp::TotalNet', value: dto.totalNet, type: 'currency' },
      { labelKey: 'Erp::PaymentVoucherNo', value: dto.paymentVoucherNo },
    ],
    actions: [
      {
        key: 'calculate',
        labelKey: 'Erp::CalculatePayroll',
        icon: 'fas fa-calculator',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status !== PayrollRunStatus.Posted,
        run: dto => this.runs.calculate(dto.id!),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === PayrollRunStatus.Calculated,
        confirmKey: 'Erp::PostPayrollConfirmation',
        run: dto => this.runs.runPosting(dto.id!),
      },
      {
        key: 'voucher',
        labelKey: 'Erp::RaisePaymentVoucher',
        icon: 'fas fa-money-check',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === PayrollRunStatus.Posted && !dto.paymentVoucherNo,
        confirmKey: 'Erp::RaisePayrollVoucherConfirmation',
        run: dto => this.runs.raisePaymentVoucher(dto.id!),
      },
    ],
    related: dto =>
      this.payslips.getList({ payrollRunNo: dto.no, maxResultCount: 1, skipCount: 0 }).pipe(
        map(payslips => [
          {
            labelKey: 'Erp::Payslips',
            icon: 'fas fa-file-invoice',
            count: payslips.totalCount ?? 0,
            routerLink: ['/erp/payslips'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
        ]),
      ),
  };

  readonly payslip: RecordEntity<PayslipDto, never> = {
    key: 'payslip',
    titleKey: 'Erp::Payslip',
    pluralKey: 'Erp::Payslips',
    icon: 'fas fa-file-invoice',
    permission: PERMISSION,
    listRoute: ['/erp/payslips'],
    readOnly: true,
    columns: [
      { field: 'payrollRunNo', labelKey: 'Erp::PayrollRunNo', width: 120 },
      { field: 'payPeriod', labelKey: 'Erp::PayPeriod', type: 'date' },
      { field: 'employeeNo', labelKey: 'Erp::EmployeeNo' },
      { field: 'employeeName', labelKey: 'Erp::EmployeeName', width: 200 },
      { field: 'grossPay', labelKey: 'Erp::GrossPay', type: 'currency' },
      { field: 'incomeTax', labelKey: 'Erp::IncomeTax', type: 'currency' },
      { field: 'totalDeductions', labelKey: 'Erp::TotalDeductions', type: 'currency' },
      { field: 'netPay', labelKey: 'Erp::NetPay', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'payrollRunNo', labelKey: 'Erp::PayrollRunNo', type: 'readonly' },
      { field: 'payPeriod', labelKey: 'Erp::PayPeriod', type: 'readonly' },
      { field: 'employeeNo', labelKey: 'Erp::EmployeeNo', type: 'readonly' },
      { field: 'employeeName', labelKey: 'Erp::EmployeeName', type: 'readonly' },
      { field: 'jobTitle', labelKey: 'Erp::JobTitle', type: 'readonly' },
      { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo', type: 'readonly' },
      { field: 'basicPay', labelKey: 'Erp::BasicPay', type: 'readonly' },
      { field: 'grossPay', labelKey: 'Erp::GrossPay', type: 'readonly' },
      { field: 'taxablePay', labelKey: 'Erp::TaxablePay', type: 'readonly' },
      { field: 'incomeTax', labelKey: 'Erp::IncomeTax', type: 'readonly' },
      { field: 'totalDeductions', labelKey: 'Erp::TotalDeductions', type: 'readonly' },
      { field: 'netPay', labelKey: 'Erp::NetPay', type: 'readonly' },
      { field: 'employerContributions', labelKey: 'Erp::TotalEmployerContributions', type: 'readonly' },
    ],
    getList: query => this.payslips.getList(query),
    get: id => this.payslips.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: `${dto.payrollRunNo ?? ''} ${dto.employeeNo ?? ''}`.trim(), name: dto.employeeName ?? undefined }),
    newRecord: () => ({}),
    facts: dto => [
      { labelKey: 'Erp::GrossPay', value: dto.grossPay, type: 'currency' },
      { labelKey: 'Erp::NetPay', value: dto.netPay, type: 'currency' },
    ],
    related: dto =>
      this.payslipLines.getList({ payrollRunNo: dto.payrollRunNo, employeeNo: dto.employeeNo, maxResultCount: 1, skipCount: 0 }).pipe(
        map(lines => [
          {
            labelKey: 'Erp::PayslipLines',
            icon: 'fas fa-list',
            count: lines.totalCount ?? 0,
            routerLink: ['/erp/payslip-lines'],
            queryParams: { filter: dto.employeeNo },
            permission: PERMISSION,
          },
        ]),
      ),
  };

  readonly payslipLine: RecordEntity<PayslipLineDto, never> = {
    key: 'payslipLine',
    titleKey: 'Erp::PayslipLine',
    pluralKey: 'Erp::PayslipLines',
    icon: 'fas fa-list',
    permission: PERMISSION,
    listRoute: ['/erp/payslip-lines'],
    readOnly: true,
    columns: [
      { field: 'payrollRunNo', labelKey: 'Erp::PayrollRunNo', width: 120 },
      { field: 'employeeNo', labelKey: 'Erp::EmployeeNo' },
      { field: 'lineType', labelKey: 'Erp::LineType', type: 'select', options: enumOptions(payslipLineTypeOptions, 'PayslipLineType') },
      { field: 'code', labelKey: 'Erp::Code' },
      { field: 'description', labelKey: 'Erp::Description', width: 220 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'payrollRunNo', labelKey: 'Erp::PayrollRunNo', type: 'readonly' },
      { field: 'employeeNo', labelKey: 'Erp::EmployeeNo', type: 'readonly' },
      { field: 'code', labelKey: 'Erp::Code', type: 'readonly' },
      { field: 'description', labelKey: 'Erp::Description', type: 'readonly' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'readonly' },
    ],
    getList: query => this.payslipLines.getList(query),
    get: id => this.payslipLines.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: `${dto.employeeNo ?? ''} ${dto.code ?? ''}`.trim(), name: PayslipLineType[dto.lineType] }),
    newRecord: () => ({}),
  };

  get all(): RecordEntity[] {
    return [this.payrollEarning, this.payrollDeduction, this.payrollTaxBand, this.employeePayItem, this.payrollRun, this.payslip, this.payslipLine];
  }
}
