import { Injectable, inject } from '@angular/core';
import {
  BenefitCalculationStatus,
  CreateUpdatePensionBenefitCalculationDto,
  PensionBenefitCalculationDto,
  PensionBenefitCalculationService,
  benefitCalculationStatusOptions,
  CreateUpdatePensionPayrollHeaderDto,
  CreateUpdatePensionPayrollLineDto,
  CreateUpdatePensionerDto,
  PensionDocumentStatus,
  PensionPayrollHeaderDto,
  PensionPayrollLineDto,
  PensionPayrollLineService,
  PensionPayrollService,
  PensionerChangeEntryService,
  PensionerDto,
  PensionerService,
  PensionerPayItemAssignmentService,
  PensionPayrollLineItemService,
  PensionerStatus,
  pensionDocumentStatusOptions,
  pensionerStatusOptions,
} from '@proxy/pensions';
import { map } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { codeField, enumOptions } from './entity-helpers';

const PERMISSION = 'Erp.Pensions';
const today = () => new Date().toISOString().substring(0, 10);

/** A read-only figure of a card: a total or what posting filled in. */
function figure(field: string, labelKey: string, section: string): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

/**
 * Pensioners and the monthly pension payroll, which is paid through a payment voucher. Joined
 * into `MasterDataEntities.all`, so every lookup can find them.
 */
@Injectable({ providedIn: 'root' })
export class PensionPayrollEntities {
  private readonly pensioners = inject(PensionerService);
  private readonly payrolls = inject(PensionPayrollService);
  private readonly payrollLines = inject(PensionPayrollLineService);
  private readonly calculations = inject(PensionBenefitCalculationService);
  private readonly changes = inject(PensionerChangeEntryService);
  private readonly payItems = inject(PensionerPayItemAssignmentService);
  private readonly lineItems = inject(PensionPayrollLineItemService);

  readonly pensioner: RecordEntity<PensionerDto, CreateUpdatePensionerDto> = {
    key: 'pensioner',
    titleKey: 'Erp::Pensioner',
    pluralKey: 'Erp::Pensioners',
    icon: 'fas fa-person-cane',
    permission: PERMISSION,
    listRoute: ['/erp/pensioners'],
    attachmentEntityType: 'Pensioner',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 110 },
      { field: 'name', labelKey: 'Erp::Name', width: 220 },
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode' },
      { field: 'memberNo', labelKey: 'Erp::MemberNo' },
      { field: 'monthlyPension', labelKey: 'Erp::MonthlyPension', type: 'currency' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(pensionerStatusOptions, 'PensionerStatus') },
      { field: 'lastPaidPeriod', labelKey: 'Erp::LastPaidPeriod', type: 'date' },
      { field: 'lifeCertificateDueDate', labelKey: 'Erp::LifeCertificateDueDate', type: 'date' },
      { field: 'arrearsAmount', labelKey: 'Erp::ArrearsAmount', type: 'currency' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'administration', labelKey: 'Erp::PensionAdministration' },
      { key: 'personal', labelKey: 'Erp::Personal', collapsed: true },
      { key: 'payments', labelKey: 'Erp::Payments', collapsed: true },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('memberNo', 'Erp::MemberNo', 'pensionMember', undefined, { helpKey: 'Erp::PensionerMemberHelp' }),
      { field: 'name', labelKey: 'Erp::Name', type: 'text', maxLength: 100 },
      codeField('schemeCode', 'Erp::SchemeCode', 'pensionScheme'),
      { field: 'monthlyPension', labelKey: 'Erp::MonthlyPension', type: 'currency', min: 0, required: true },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date', required: true },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date', cardOnly: true },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(pensionerStatusOptions, 'PensionerStatus') },
      { field: 'taxExempt', labelKey: 'Erp::TaxExempt', type: 'checkbox', cardOnly: true },
      figure('lastPaidPeriod', 'Erp::LastPaidPeriod', 'general'),
      figure('suspensionReasonCode', 'Erp::SuspensionReasonCode', 'administration'),
      figure('suspensionReason', 'Erp::SuspensionReason', 'administration'),
      figure('arrearsAmount', 'Erp::ArrearsAmount', 'administration'),
      figure('arrearsMonths', 'Erp::ArrearsMonths', 'administration'),
      figure('lastLifeCertificateDate', 'Erp::LastLifeCertificateDate', 'administration'),
      figure('lifeCertificateDueDate', 'Erp::LifeCertificateDueDate', 'administration'),
      { field: 'nationalId', labelKey: 'Erp::NationalId', type: 'text', maxLength: 40, section: 'personal', cardOnly: true },
      { field: 'taxPinNo', labelKey: 'Erp::TaxPinNo', type: 'text', maxLength: 20, section: 'personal', cardOnly: true },
      { field: 'dateOfBirth', labelKey: 'Erp::DateOfBirth', type: 'date', section: 'personal', cardOnly: true },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30, section: 'personal', cardOnly: true },
      { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80, section: 'personal', cardOnly: true },
      codeField('payModeCode', 'Erp::PayModeCode', 'pensionerPayMode', 'payments', { cardOnly: true, helpKey: 'Erp::PayModeCodeHelp' }),
      codeField('bankCode', 'Erp::BankCode', 'pensionBank', 'payments', { cardOnly: true, helpKey: 'Erp::PensionerBankCodeHelp' }),
      codeField('bankBranchCode', 'Erp::BranchCode', 'pensionBankBranch', 'payments', { cardOnly: true }),
      { field: 'bankName', labelKey: 'Erp::BankName', type: 'text', maxLength: 100, section: 'payments', cardOnly: true },
      { field: 'bankBranch', labelKey: 'Erp::BankBranch', type: 'text', maxLength: 100, section: 'payments', cardOnly: true },
      { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo', type: 'text', maxLength: 30, section: 'payments', cardOnly: true },
    ],
    getList: query => this.pensioners.getList(query),
    get: id => this.pensioners.get(id),
    create: input => this.pensioners.create(input),
    update: (id, input) => this.pensioners.update(id, input),
    delete: id => this.pensioners.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.name ?? undefined }),
    newRecord: term => ({ name: term ?? '', monthlyPension: 0, startDate: today(), status: PensionerStatus.Active, taxExempt: false }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: dto.status === PensionerStatus.Suspended ? `${PensionerStatus[dto.status]}: ${dto.suspensionReason ?? ''}` : PensionerStatus[dto.status] },
      { labelKey: 'Erp::MonthlyPension', value: dto.monthlyPension, type: 'currency' },
      { labelKey: 'Erp::ArrearsAmount', value: dto.arrearsAmount, type: 'currency' },
    ],
    actions: [
      {
        key: 'lifeCertificate',
        labelKey: 'Erp::RecordLifeCertificate',
        icon: 'fas fa-file-signature',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status !== PensionerStatus.Ceased,
        confirmKey: 'Erp::RecordLifeCertificateConfirmation',
        run: dto => this.pensioners.recordLifeCertificate(dto.id!, {}),
      },
      {
        key: 'suspend',
        labelKey: 'Erp::SuspendPension',
        icon: 'fas fa-pause',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === PensionerStatus.Active,
        confirmKey: 'Erp::SuspendPensionConfirmation',
        run: dto => this.pensioners.suspend(dto.id!, {}),
      },
      {
        key: 'reinstate',
        labelKey: 'Erp::ReinstatePension',
        icon: 'fas fa-play',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === PensionerStatus.Suspended,
        confirmKey: 'Erp::ReinstatePensionConfirmation',
        run: dto => this.pensioners.reinstate(dto.id!, {}),
      },
    ],
    parts: [
      {
        // Paid or taken with every payroll for a month between the dates.
        entity: 'pensionerPayItemAssignment',
        lines: dto => this.payItems.getList({ pensionerNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ pensionerNo: dto.no }),
        columns: ['payItemCode', 'payItemDescription', 'itemType', 'amount', 'startDate', 'endDate'],
      },
      {
        entity: 'pensionerChangeEntry',
        lines: dto => this.changes.getList({ pensionerNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        columns: ['effectiveDate', 'changeType', 'oldMonthlyPension', 'newMonthlyPension', 'amount', 'description'],
        editable: () => false,
      },
    ],
    related: dto =>
      this.payrollLines.getList({ pensionerNo: dto.no, maxResultCount: 1, skipCount: 0 }).pipe(
        map(lines => [
          {
            labelKey: 'Erp::PensionPayrollLines',
            icon: 'fas fa-list',
            count: lines.totalCount ?? 0,
            routerLink: ['/erp/pension-payroll-lines'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
        ]),
      ),
  };

  readonly pensionPayroll: RecordEntity<PensionPayrollHeaderDto, CreateUpdatePensionPayrollHeaderDto> = {
    key: 'pensionPayroll',
    titleKey: 'Erp::PensionPayroll',
    pluralKey: 'Erp::PensionPayrolls',
    icon: 'fas fa-money-bill-wave',
    permission: PERMISSION,
    listRoute: ['/erp/pension-payrolls'],
    parts: [
      {
        entity: 'pensionPayrollLine',
        lines: dto => this.payrollLines.getList({ documentNo: dto.no, sorting: 'lineNo', maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ documentNo: dto.no }),
        columns: ['pensionerNo', 'pensionerName', 'monthlyPension', 'arrearsAmount', 'otherEarnings', 'grossPension', 'taxAmount', 'deductions', 'netPension'],
        totals: ['grossPension', 'taxAmount', 'deductions', 'netPension'],
        editable: dto => dto.status === PensionDocumentStatus.Open,
      },
    ],
    attachmentEntityType: 'PensionPayrollHeader',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode' },
      { field: 'payPeriod', labelKey: 'Erp::PayPeriod', type: 'date' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(pensionDocumentStatusOptions, 'PensionDocumentStatus') },
      { field: 'noOfPensioners', labelKey: 'Erp::NoOfPensioners', type: 'number' },
      { field: 'totalGross', labelKey: 'Erp::TotalGross', type: 'currency' },
      { field: 'totalTax', labelKey: 'Erp::TotalTax', type: 'currency' },
      { field: 'totalDeductions', labelKey: 'Erp::TotalDeductions', type: 'currency' },
      { field: 'totalNet', labelKey: 'Erp::TotalNet', type: 'currency' },
      { field: 'paymentVoucherNo', labelKey: 'Erp::PaymentVoucherNo' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('schemeCode', 'Erp::SchemeCode', 'pensionScheme', undefined, { required: true }),
      { field: 'payPeriod', labelKey: 'Erp::PayPeriod', type: 'date', required: true, helpKey: 'Erp::PayPeriodHelp' },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
      { field: 'taxRatePct', labelKey: 'Erp::TaxRatePct', type: 'number', min: 0, helpKey: 'Erp::PayrollTaxHelp' },
      { field: 'taxFreeAmount', labelKey: 'Erp::TaxFreeAmount', type: 'currency', min: 0 },
      codeField('taxTableCode', 'Erp::TaxTableCode', 'lumpsumTaxTable', undefined, { helpKey: 'Erp::PayrollTaxTableHelp', cardOnly: true }),
      { field: 'personalRelief', labelKey: 'Erp::PersonalRelief', type: 'currency', min: 0, cardOnly: true },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      figure('paymentVoucherNo', 'Erp::PaymentVoucherNo', 'general'),
      figure('postedBy', 'Erp::PostedBy', 'general'),
    ],
    getList: query => this.payrolls.getList(query),
    get: id => this.payrolls.get(id),
    create: input => this.payrolls.create(input),
    update: (id, input) => this.payrolls.update(id, input),
    delete: id => this.payrolls.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: `${dto.schemeCode ?? ''} ${dto.payPeriod?.substring(0, 7) ?? ''}`.trim() }),
    newRecord: () => ({ payPeriod: today(), postingDate: today(), taxRatePct: 0, taxFreeAmount: 0, personalRelief: 0 }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: PensionDocumentStatus[dto.status] },
      { labelKey: 'Erp::NoOfPensioners', value: dto.noOfPensioners, type: 'number' },
      { labelKey: 'Erp::TotalGross', value: dto.totalGross, type: 'currency' },
      { labelKey: 'Erp::TotalTax', value: dto.totalTax, type: 'currency' },
      { labelKey: 'Erp::TotalDeductions', value: dto.totalDeductions, type: 'currency' },
      { labelKey: 'Erp::TotalNet', value: dto.totalNet, type: 'currency' },
    ],
    actions: [
      {
        key: 'suggest',
        labelKey: 'Erp::SuggestLines',
        icon: 'fas fa-wand-magic-sparkles',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === PensionDocumentStatus.Open,
        run: dto => this.payrolls.suggestLines(dto.id!),
      },
      {
        key: 'release',
        labelKey: 'Erp::Release',
        icon: 'fas fa-lock',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === PensionDocumentStatus.Open,
        run: dto => this.payrolls.release(dto.id!),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-lock-open',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === PensionDocumentStatus.Released,
        run: dto => this.payrolls.reopen(dto.id!),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === PensionDocumentStatus.Released,
        confirmKey: 'Erp::PostPensionPayrollConfirmation',
        run: dto => this.payrolls.runPosting(dto.id!),
      },
      {
        key: 'voucher',
        labelKey: 'Erp::RaisePaymentVoucher',
        icon: 'fas fa-money-check-dollar',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === PensionDocumentStatus.Posted && !dto.paymentVoucherNo,
        confirmKey: 'Erp::RaisePaymentVoucherConfirmation',
        run: dto => this.payrolls.raisePaymentVoucher(dto.id!),
      },
    ],
    related: dto =>
      this.payrollLines.getList({ documentNo: dto.no, maxResultCount: 1, skipCount: 0 }).pipe(
        map(lines => [
          {
            labelKey: 'Erp::PensionPayrollLines',
            icon: 'fas fa-list',
            count: lines.totalCount ?? 0,
            routerLink: ['/erp/pension-payroll-lines'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          ...(dto.paymentVoucherNo
            ? [
                {
                  labelKey: 'Erp::PaymentVoucher',
                  icon: 'fas fa-money-check-dollar',
                  routerLink: ['/erp/payment-vouchers'],
                  queryParams: { filter: dto.paymentVoucherNo },
                  permission: 'Erp.PaymentVouchers',
                },
              ]
            : []),
        ]),
      ),
  };

  readonly pensionPayrollLine: RecordEntity<PensionPayrollLineDto, CreateUpdatePensionPayrollLineDto> = {
    key: 'pensionPayrollLine',
    titleKey: 'Erp::PensionPayrollLine',
    pluralKey: 'Erp::PensionPayrollLines',
    icon: 'fas fa-list',
    permission: PERMISSION,
    listRoute: ['/erp/pension-payroll-lines'],
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'pensionerNo', labelKey: 'Erp::PensionerNo' },
      { field: 'pensionerName', labelKey: 'Erp::PensionerName', width: 220 },
      { field: 'monthlyPension', labelKey: 'Erp::MonthlyPension', type: 'currency' },
      { field: 'arrearsAmount', labelKey: 'Erp::ArrearsAmount', type: 'currency' },
      { field: 'otherEarnings', labelKey: 'Erp::OtherEarnings', type: 'currency' },
      { field: 'grossPension', labelKey: 'Erp::GrossPension', type: 'currency' },
      { field: 'taxAmount', labelKey: 'Erp::TaxAmount', type: 'currency' },
      { field: 'deductions', labelKey: 'Erp::Deductions', type: 'currency' },
      { field: 'netPension', labelKey: 'Erp::NetPension', type: 'currency' },
      { field: 'payModeCode', labelKey: 'Erp::PayModeCode' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('documentNo', 'Erp::DocumentNo', 'pensionPayroll', undefined, { required: true, createOnly: true }),
      codeField('pensionerNo', 'Erp::PensionerNo', 'pensioner', undefined, { required: true }),
      { field: 'monthlyPension', labelKey: 'Erp::MonthlyPension', type: 'currency', min: 0, helpKey: 'Erp::PayrollLineMonthlyPensionHelp' },
      { field: 'taxAmount', labelKey: 'Erp::TaxAmount', type: 'currency', min: 0 },
      figure('arrearsAmount', 'Erp::ArrearsAmount', 'general'),
      figure('arrearsMonths', 'Erp::ArrearsMonths', 'general'),
      figure('otherEarnings', 'Erp::OtherEarnings', 'general'),
      figure('grossPension', 'Erp::GrossPension', 'general'),
      figure('deductions', 'Erp::Deductions', 'general'),
      figure('netPension', 'Erp::NetPension', 'general'),
      figure('payModeCode', 'Erp::PayModeCode', 'general'),
    ],
    getList: query => this.payrollLines.getList(query),
    get: id => this.payrollLines.get(id),
    create: input => this.payrollLines.create(input),
    update: (id, input) => this.payrollLines.update(id, input),
    delete: id => this.payrollLines.delete(id),
    // A blank amount means "work it out", which the server reads as a missing value.
    toInput: value =>
      Object.fromEntries(Object.entries(value).map(([key, v]) => [key, v === '' || v === null ? undefined : v])) as unknown as CreateUpdatePensionPayrollLineDto,
    toItem: dto => ({ id: dto.id, code: `${dto.documentNo ?? ''} ${dto.pensionerNo ?? ''}`.trim(), name: dto.pensionerName ?? undefined }),
    // Blank, not zero: the server works the pension and its tax out.
    newRecord: () => ({ monthlyPension: null, taxAmount: null }) as Partial<PensionPayrollLineDto>,
    // The earnings and deductions the line was worked out with.
    parts: [
      {
        entity: 'pensionPayrollLineItem',
        lines: dto =>
          this.lineItems.getList({ documentNo: dto.documentNo, lineNo: dto.lineNo, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        columns: ['payItemCode', 'description', 'itemType', 'taxable', 'amount'],
      },
    ],
  };

  readonly pensionBenefitCalculation: RecordEntity<PensionBenefitCalculationDto, CreateUpdatePensionBenefitCalculationDto> = {
    key: 'pensionBenefitCalculation',
    titleKey: 'Erp::PensionBenefitCalculation',
    pluralKey: 'Erp::PensionBenefitCalculations',
    icon: 'fas fa-calculator',
    permission: PERMISSION,
    listRoute: ['/erp/pension-benefit-calculations'],
    attachmentEntityType: 'PensionBenefitCalculation',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'memberNo', labelKey: 'Erp::MemberNo' },
      { field: 'memberName', labelKey: 'Erp::MemberName', width: 200 },
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode' },
      { field: 'retirementDate', labelKey: 'Erp::RetirementDate', type: 'date' },
      { field: 'monthlyPension', labelKey: 'Erp::MonthlyPension', type: 'currency' },
      { field: 'lumpSum', labelKey: 'Erp::LumpSum', type: 'currency' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(benefitCalculationStatusOptions, 'BenefitCalculationStatus') },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'result', labelKey: 'Erp::Calculation' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('memberNo', 'Erp::MemberNo', 'pensionMember', undefined, { required: true }),
      { field: 'calculationDate', labelKey: 'Erp::CalculationDate', type: 'date', required: true },
      { field: 'retirementDate', labelKey: 'Erp::RetirementDate', type: 'date', helpKey: 'Erp::RetirementDateHelp' },
      { field: 'finalPensionableSalary', labelKey: 'Erp::FinalPensionableSalary', type: 'currency', min: 0, helpKey: 'Erp::FinalPensionableSalaryHelp' },
      { field: 'commutationPct', labelKey: 'Erp::CommutationPct', type: 'number', min: 0, helpKey: 'Erp::CommutationPctHelp' },
      { field: 'comment', labelKey: 'Erp::Comment', type: 'text', maxLength: 250, wide: true, cardOnly: true },
      figure('ageAtRetirement', 'Erp::AgeAtRetirement', 'result'),
      figure('pensionableServiceYears', 'Erp::PensionableServiceYears', 'result'),
      figure('accrualRatePct', 'Erp::AccrualRatePct', 'result'),
      figure('earlyReductionPct', 'Erp::EarlyReductionPct', 'result'),
      figure('ageFactor', 'Erp::AgeFactor', 'result'),
      figure('trivial', 'Erp::TrivialPension', 'result'),
      figure('fullAnnualPension', 'Erp::FullAnnualPension', 'result'),
      figure('reducedAnnualPension', 'Erp::ReducedAnnualPension', 'result'),
      figure('commutedAnnualPension', 'Erp::CommutedAnnualPension', 'result'),
      figure('commutationFactor', 'Erp::CommutationFactor', 'result'),
      figure('lumpSum', 'Erp::LumpSum', 'result'),
      figure('annualPension', 'Erp::AnnualPension', 'result'),
      figure('monthlyPension', 'Erp::MonthlyPension', 'result'),
      figure('pensionerNo', 'Erp::PensionerNo', 'result'),
      figure('paymentVoucherNo', 'Erp::PaymentVoucherNo', 'result'),
    ],
    getList: query => this.calculations.getList(query),
    get: id => this.calculations.get(id),
    create: input => this.calculations.create(input),
    update: (id, input) => this.calculations.update(id, input),
    delete: id => this.calculations.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.memberName ?? undefined }),
    newRecord: () => ({ calculationDate: today(), finalPensionableSalary: 0, commutationPct: 0 }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: BenefitCalculationStatus[dto.status] },
      { labelKey: 'Erp::MonthlyPension', value: dto.monthlyPension, type: 'currency' },
      { labelKey: 'Erp::LumpSum', value: dto.lumpSum, type: 'currency' },
    ],
    actions: [
      {
        key: 'calculate',
        labelKey: 'Erp::Recalculate',
        icon: 'fas fa-calculator',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === BenefitCalculationStatus.Open,
        run: dto => this.calculations.calculate(dto.id!),
      },
      {
        key: 'approve',
        labelKey: 'Erp::Approve',
        icon: 'fas fa-check',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === BenefitCalculationStatus.Open,
        confirmKey: 'Erp::ApproveBenefitCalculationConfirmation',
        run: dto => this.calculations.approve(dto.id!),
      },
    ],
  };

  get all(): RecordEntity[] {
    return [this.pensioner, this.pensionPayroll, this.pensionPayrollLine, this.pensionBenefitCalculation];
  }
}
