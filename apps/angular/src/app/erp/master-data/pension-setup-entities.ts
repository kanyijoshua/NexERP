import { Injectable, inject } from '@angular/core';
import {
  CreateUpdateExitReasonDocumentDto,
  CreateUpdateMemberExitDocumentDto,
  CreateUpdatePensionAgeFactorDto,
  CreateUpdatePensionBankBranchDto,
  CreateUpdatePensionerPayItemAssignmentDto,
  ExitReasonDocumentDto,
  ExitReasonDocumentService,
  MemberExitDocumentDto,
  MemberExitDocumentService,
  OtherPensionSchemeService,
  PensionAgeFactorDto,
  PensionAgeFactorService,
  PensionBankBranchDto,
  PensionBankBranchService,
  PensionBankService,
  PensionFactorType,
  PensionPayrollLineItemDto,
  PensionPayrollLineItemService,
  PensionRevisionReasonService,
  PensionerPayItemAssignmentDto,
  PensionerPayItemAssignmentService,
  PensionerPayItemCalculation,
  PensionerPayItemService,
  PensionerPayItemType,
  PensionerPayModeService,
  PensionerPaymentType,
  PensionerSuspensionReasonService,
  pensionFactorTypeOptions,
  pensionerPayItemCalculationOptions,
  pensionerPayItemTypeOptions,
  pensionerPaymentTypeOptions,
} from '@proxy/pensions';
import { EMPTY, map } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { codeField, codeTableEntity, enumOptions } from './entity-helpers';

const PERMISSION = 'Erp.Pensions';
const SETUP_PERMISSION = 'Erp.PensionSetup';
const firstOfMonth = () => new Date().toISOString().substring(0, 8) + '01';

/** A read-only figure of a card: what the system filled in. */
function figure(field: string, labelKey: string, section = 'general'): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

/** A blank date or code is "none" to the server, not an empty string it cannot read. */
function withoutBlanks<T>(value: Record<string, unknown>): T {
  return Object.fromEntries(Object.entries(value).map(([key, v]) => [key, v === '' ? undefined : v])) as T;
}

/**
 * The pension setup tables: banks and their branches, pay modes, suspension and revision reasons,
 * other schemes, pensioners' earnings and deductions, the documents exits need and the age
 * factors of defined benefit schemes. Joined into `MasterDataEntities.all`, so every lookup can
 * find them.
 */
@Injectable({ providedIn: 'root' })
export class PensionSetupEntities {
  private readonly banks = inject(PensionBankService);
  private readonly branches = inject(PensionBankBranchService);
  private readonly payModes = inject(PensionerPayModeService);
  private readonly suspensionReasons = inject(PensionerSuspensionReasonService);
  private readonly revisionReasons = inject(PensionRevisionReasonService);
  private readonly otherSchemes = inject(OtherPensionSchemeService);
  private readonly payItems = inject(PensionerPayItemService);
  private readonly assignments = inject(PensionerPayItemAssignmentService);
  private readonly lineItems = inject(PensionPayrollLineItemService);
  private readonly reasonDocuments = inject(ExitReasonDocumentService);
  private readonly exitDocuments = inject(MemberExitDocumentService);
  private readonly factors = inject(PensionAgeFactorService);

  readonly pensionBank = codeTableEntity(this.banks, {
    key: 'pensionBank',
    titleKey: 'Erp::PensionBank',
    pluralKey: 'Erp::PensionBanks',
    icon: 'fas fa-building-columns',
    permission: SETUP_PERMISSION,
    route: '/erp/pension-banks',
    descriptionKey: 'Erp::Name',
    columns: [{ field: 'swiftCode', labelKey: 'Erp::SwiftCode' }],
    fields: [{ field: 'swiftCode', labelKey: 'Erp::SwiftCode', type: 'text', maxLength: 20 }],
  });

  readonly pensionBankBranch: RecordEntity<PensionBankBranchDto, CreateUpdatePensionBankBranchDto> = {
    key: 'pensionBankBranch',
    titleKey: 'Erp::PensionBankBranch',
    pluralKey: 'Erp::PensionBankBranches',
    icon: 'fas fa-code-branch',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/pension-bank-branches'],
    columns: [
      { field: 'bankCode', labelKey: 'Erp::BankCode', width: 110 },
      { field: 'branchCode', labelKey: 'Erp::BranchCode', width: 110 },
      { field: 'name', labelKey: 'Erp::Name', width: 240 },
      { field: 'swiftCode', labelKey: 'Erp::SwiftCode' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('bankCode', 'Erp::BankCode', 'pensionBank', undefined, { required: true }),
      { field: 'branchCode', labelKey: 'Erp::BranchCode', type: 'text', required: true, maxLength: 20 },
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      { field: 'swiftCode', labelKey: 'Erp::SwiftCode', type: 'text', maxLength: 20 },
    ],
    getList: query => this.branches.getList(query),
    get: id => this.branches.get(id),
    create: input => this.branches.create(input),
    update: (id, input) => this.branches.update(id, input),
    delete: id => this.branches.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.branchCode ?? '', name: `${dto.bankCode ?? ''} ${dto.name ?? ''}`.trim() }),
    newRecord: () => ({}),
  };

  readonly pensionerPayMode = codeTableEntity(this.payModes, {
    key: 'pensionerPayMode',
    titleKey: 'Erp::PensionerPayMode',
    pluralKey: 'Erp::PensionerPayModes',
    icon: 'fas fa-money-bill-transfer',
    permission: SETUP_PERMISSION,
    route: '/erp/pensioner-pay-modes',
    columns: [{ field: 'paymentType', labelKey: 'Erp::PaymentType', type: 'select', options: enumOptions(pensionerPaymentTypeOptions, 'PensionerPaymentType') }],
    fields: [{ field: 'paymentType', labelKey: 'Erp::PaymentType', type: 'select', options: enumOptions(pensionerPaymentTypeOptions, 'PensionerPaymentType') }],
    defaults: { paymentType: PensionerPaymentType.Bank },
  });

  readonly pensionerSuspensionReason = codeTableEntity(this.suspensionReasons, {
    key: 'pensionerSuspensionReason',
    titleKey: 'Erp::PensionerSuspensionReason',
    pluralKey: 'Erp::PensionerSuspensionReasons',
    icon: 'fas fa-circle-pause',
    permission: SETUP_PERMISSION,
    route: '/erp/pensioner-suspension-reasons',
    columns: [{ field: 'lifeCertificate', labelKey: 'Erp::LifeCertificateReason', type: 'boolean' }],
    fields: [{ field: 'lifeCertificate', labelKey: 'Erp::LifeCertificateReason', type: 'checkbox', helpKey: 'Erp::LifeCertificateReasonHelp' }],
    defaults: { lifeCertificate: false },
  });

  readonly pensionRevisionReason = codeTableEntity(this.revisionReasons, {
    key: 'pensionRevisionReason',
    titleKey: 'Erp::PensionRevisionReason',
    pluralKey: 'Erp::PensionRevisionReasons',
    icon: 'fas fa-pen-ruler',
    permission: SETUP_PERMISSION,
    route: '/erp/pension-revision-reasons',
  });

  readonly otherPensionScheme = codeTableEntity(this.otherSchemes, {
    key: 'otherPensionScheme',
    titleKey: 'Erp::OtherPensionScheme',
    pluralKey: 'Erp::OtherPensionSchemes',
    icon: 'fas fa-right-left',
    permission: SETUP_PERMISSION,
    route: '/erp/other-pension-schemes',
    descriptionKey: 'Erp::Name',
    columns: [
      { field: 'contactName', labelKey: 'Erp::ContactName' },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'contact', labelKey: 'Erp::AddressAndContact' },
      { key: 'payments', labelKey: 'Erp::Payments', collapsed: true },
    ],
    fields: [
      { field: 'regulatorReferenceNo', labelKey: 'Erp::RegulatorReferenceNo', type: 'text', maxLength: 35 },
      { field: 'address', labelKey: 'Erp::Address', type: 'text', maxLength: 100, section: 'contact', cardOnly: true },
      { field: 'city', labelKey: 'Erp::City', type: 'text', maxLength: 50, section: 'contact', cardOnly: true },
      { field: 'contactName', labelKey: 'Erp::ContactName', type: 'text', maxLength: 100, section: 'contact' },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30, section: 'contact' },
      { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80, section: 'contact', cardOnly: true },
      codeField('bankCode', 'Erp::BankCode', 'pensionBank', 'payments', { cardOnly: true }),
      codeField('bankBranchCode', 'Erp::BranchCode', 'pensionBankBranch', 'payments', { cardOnly: true }),
      { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo', type: 'text', maxLength: 30, section: 'payments', cardOnly: true },
    ],
  });

  readonly pensionerPayItem = codeTableEntity(this.payItems, {
    key: 'pensionerPayItem',
    titleKey: 'Erp::PensionerPayItem',
    pluralKey: 'Erp::PensionerPayItems',
    icon: 'fas fa-list-check',
    permission: SETUP_PERMISSION,
    route: '/erp/pensioner-pay-items',
    columns: [
      { field: 'itemType', labelKey: 'Erp::ItemType', type: 'select', options: enumOptions(pensionerPayItemTypeOptions, 'PensionerPayItemType') },
      { field: 'calculation', labelKey: 'Erp::CalculationMethod', type: 'select', options: enumOptions(pensionerPayItemCalculationOptions, 'PensionerPayItemCalculation') },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
      { field: 'pct', labelKey: 'Erp::Pct', type: 'number' },
      { field: 'taxable', labelKey: 'Erp::Taxable', type: 'boolean' },
      { field: 'accountNo', labelKey: 'Erp::AccountNo' },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean' },
    ],
    fields: [
      { field: 'itemType', labelKey: 'Erp::ItemType', type: 'select', options: enumOptions(pensionerPayItemTypeOptions, 'PensionerPayItemType') },
      {
        field: 'calculation',
        labelKey: 'Erp::CalculationMethod',
        type: 'select',
        options: enumOptions(pensionerPayItemCalculationOptions, 'PensionerPayItemCalculation'),
      },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', min: 0, helpKey: 'Erp::PensionerPayItemAmountHelp' },
      { field: 'pct', labelKey: 'Erp::Pct', type: 'number', min: 0, helpKey: 'Erp::PayItemPctHelp' },
      { field: 'taxable', labelKey: 'Erp::Taxable', type: 'checkbox', helpKey: 'Erp::PayItemTaxableHelp' },
      codeField('accountNo', 'Erp::AccountNo', 'glAccount', undefined, { helpKey: 'Erp::PayItemAccountHelp' }),
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    defaults: {
      itemType: PensionerPayItemType.Deduction,
      calculation: PensionerPayItemCalculation.FlatAmount,
      amount: 0,
      pct: 0,
      taxable: false,
      blocked: false,
    },
    quickCreate: false,
  });

  readonly pensionerPayItemAssignment: RecordEntity<PensionerPayItemAssignmentDto, CreateUpdatePensionerPayItemAssignmentDto> = {
    key: 'pensionerPayItemAssignment',
    titleKey: 'Erp::PensionerPayItemAssignment',
    pluralKey: 'Erp::PensionerPayItemAssignments',
    icon: 'fas fa-hand-holding-dollar',
    permission: PERMISSION,
    listRoute: ['/erp/pensioner-pay-item-assignments'],
    columns: [
      { field: 'pensionerNo', labelKey: 'Erp::PensionerNo', width: 110 },
      { field: 'payItemCode', labelKey: 'Erp::PayItemCode', width: 120 },
      { field: 'payItemDescription', labelKey: 'Erp::Description', width: 200 },
      { field: 'itemType', labelKey: 'Erp::ItemType', type: 'select', options: enumOptions(pensionerPayItemTypeOptions, 'PensionerPayItemType') },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('pensionerNo', 'Erp::PensionerNo', 'pensioner', undefined, { required: true, createOnly: true }),
      codeField('payItemCode', 'Erp::PayItemCode', 'pensionerPayItem', undefined, { required: true }),
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', min: 0, helpKey: 'Erp::AssignmentAmountHelp' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
      { field: 'comment', labelKey: 'Erp::Comment', type: 'text', maxLength: 250, wide: true, cardOnly: true },
    ],
    getList: query => this.assignments.getList(query),
    get: id => this.assignments.get(id),
    create: input => this.assignments.create(input),
    update: (id, input) => this.assignments.update(id, input),
    delete: id => this.assignments.delete(id),
    toInput: value => withoutBlanks<CreateUpdatePensionerPayItemAssignmentDto>(value),
    toItem: dto => ({ id: dto.id, code: `${dto.pensionerNo ?? ''} ${dto.payItemCode ?? ''}`.trim(), name: dto.payItemDescription ?? undefined }),
    newRecord: () => ({ amount: 0, startDate: firstOfMonth() }),
  };

  readonly pensionPayrollLineItem: RecordEntity<PensionPayrollLineItemDto, never> = {
    key: 'pensionPayrollLineItem',
    titleKey: 'Erp::PensionPayrollLineItem',
    pluralKey: 'Erp::PensionPayrollLineItems',
    icon: 'fas fa-receipt',
    permission: PERMISSION,
    listRoute: ['/erp/pension-payroll-line-items'],
    readOnly: true,
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'pensionerNo', labelKey: 'Erp::PensionerNo' },
      { field: 'payItemCode', labelKey: 'Erp::PayItemCode' },
      { field: 'description', labelKey: 'Erp::Description', width: 200 },
      { field: 'itemType', labelKey: 'Erp::ItemType', type: 'select', options: enumOptions(pensionerPayItemTypeOptions, 'PensionerPayItemType') },
      { field: 'taxable', labelKey: 'Erp::Taxable', type: 'boolean' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      figure('documentNo', 'Erp::DocumentNo'),
      figure('pensionerNo', 'Erp::PensionerNo'),
      figure('payItemCode', 'Erp::PayItemCode'),
      figure('description', 'Erp::Description'),
      figure('amount', 'Erp::Amount'),
      figure('accountNo', 'Erp::AccountNo'),
    ],
    getList: query => this.lineItems.getList(query),
    get: id => this.lineItems.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: `${dto.documentNo ?? ''} ${dto.payItemCode ?? ''}`.trim(), name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  readonly exitReasonDocument: RecordEntity<ExitReasonDocumentDto, CreateUpdateExitReasonDocumentDto> = {
    key: 'exitReasonDocument',
    titleKey: 'Erp::ExitReasonDocument',
    pluralKey: 'Erp::ExitReasonDocuments',
    icon: 'fas fa-file-circle-check',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/exit-reason-documents'],
    columns: [
      { field: 'exitReasonCode', labelKey: 'Erp::ReasonCode', width: 120 },
      { field: 'documentName', labelKey: 'Erp::DocumentName', width: 260 },
      { field: 'mandatory', labelKey: 'Erp::Mandatory', type: 'boolean' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('exitReasonCode', 'Erp::ReasonCode', 'exitReason', undefined, { required: true, createOnly: true }),
      { field: 'documentName', labelKey: 'Erp::DocumentName', type: 'text', required: true, maxLength: 100 },
      { field: 'mandatory', labelKey: 'Erp::Mandatory', type: 'checkbox', helpKey: 'Erp::MandatoryDocumentHelp' },
    ],
    getList: query => this.reasonDocuments.getList(query),
    get: id => this.reasonDocuments.get(id),
    create: input => this.reasonDocuments.create(input),
    update: (id, input) => this.reasonDocuments.update(id, input),
    delete: id => this.reasonDocuments.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.exitReasonCode ?? ''} ${dto.lineNo}`.trim(), name: dto.documentName ?? undefined }),
    newRecord: () => ({ mandatory: true }),
  };

  readonly memberExitDocument: RecordEntity<MemberExitDocumentDto, CreateUpdateMemberExitDocumentDto> = {
    key: 'memberExitDocument',
    titleKey: 'Erp::MemberExitDocument',
    pluralKey: 'Erp::MemberExitDocuments',
    icon: 'fas fa-folder-open',
    permission: PERMISSION,
    listRoute: ['/erp/member-exit-documents'],
    columns: [
      { field: 'exitNo', labelKey: 'Erp::ExitNo', width: 120 },
      { field: 'documentName', labelKey: 'Erp::DocumentName', width: 240 },
      { field: 'mandatory', labelKey: 'Erp::Mandatory', type: 'boolean' },
      { field: 'received', labelKey: 'Erp::Received', type: 'boolean' },
      { field: 'receivedDate', labelKey: 'Erp::ReceivedDate', type: 'date' },
      { field: 'remarks', labelKey: 'Erp::Remarks', width: 220 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('exitNo', 'Erp::ExitNo', 'memberExit', undefined, { required: true, createOnly: true }),
      { field: 'documentName', labelKey: 'Erp::DocumentName', type: 'text', required: true, maxLength: 100 },
      { field: 'mandatory', labelKey: 'Erp::Mandatory', type: 'checkbox' },
      { field: 'received', labelKey: 'Erp::Received', type: 'checkbox' },
      { field: 'receivedDate', labelKey: 'Erp::ReceivedDate', type: 'date', helpKey: 'Erp::ReceivedDateHelp' },
      { field: 'remarks', labelKey: 'Erp::Remarks', type: 'text', maxLength: 250 },
    ],
    getList: query => this.exitDocuments.getList(query),
    get: id => this.exitDocuments.get(id),
    create: input => this.exitDocuments.create(input),
    update: (id, input) => this.exitDocuments.update(id, input),
    delete: id => this.exitDocuments.delete(id),
    toInput: value => withoutBlanks<CreateUpdateMemberExitDocumentDto>(value),
    toItem: dto => ({ id: dto.id, code: `${dto.exitNo ?? ''} ${dto.lineNo}`.trim(), name: dto.documentName ?? undefined }),
    newRecord: () => ({ mandatory: false, received: false }),
  };

  readonly pensionAgeFactor: RecordEntity<PensionAgeFactorDto, CreateUpdatePensionAgeFactorDto> = {
    key: 'pensionAgeFactor',
    titleKey: 'Erp::PensionAgeFactor',
    pluralKey: 'Erp::PensionAgeFactors',
    icon: 'fas fa-table-cells',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/pension-age-factors'],
    columns: [
      { field: 'schemeCode', labelKey: 'Erp::SchemeCode', width: 110 },
      { field: 'factorType', labelKey: 'Erp::FactorType', type: 'select', options: enumOptions(pensionFactorTypeOptions, 'PensionFactorType') },
      { field: 'age', labelKey: 'Erp::Age', type: 'number' },
      { field: 'maleFactor', labelKey: 'Erp::MaleFactor', type: 'number' },
      { field: 'femaleFactor', labelKey: 'Erp::FemaleFactor', type: 'number' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('schemeCode', 'Erp::SchemeCode', 'pensionScheme', undefined, { required: true }),
      {
        field: 'factorType',
        labelKey: 'Erp::FactorType',
        type: 'select',
        options: enumOptions(pensionFactorTypeOptions, 'PensionFactorType'),
        helpKey: 'Erp::FactorTypeHelp',
      },
      { field: 'age', labelKey: 'Erp::Age', type: 'number', min: 0 },
      { field: 'maleFactor', labelKey: 'Erp::MaleFactor', type: 'number', min: 0 },
      { field: 'femaleFactor', labelKey: 'Erp::FemaleFactor', type: 'number', min: 0 },
    ],
    getList: query => this.factors.getList(query),
    get: id => this.factors.get(id),
    create: input => this.factors.create(input),
    update: (id, input) => this.factors.update(id, input),
    delete: id => this.factors.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.schemeCode ?? ''} ${PensionFactorType[dto.factorType] ?? ''} ${dto.age}`.trim() }),
    newRecord: () => ({ factorType: PensionFactorType.EarlyRetirement, age: 55, maleFactor: 1, femaleFactor: 1 }),
  };

  constructor() {
    // A bank's branches are kept under it.
    this.pensionBank.parts = [
      {
        entity: 'pensionBankBranch',
        lines: dto => this.branches.getList({ bankCode: dto.code, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ bankCode: dto.code }),
        columns: ['branchCode', 'name', 'swiftCode'],
      },
    ];
  }

  get all(): RecordEntity[] {
    return [
      this.pensionBank,
      this.pensionBankBranch,
      this.pensionerPayMode,
      this.pensionerSuspensionReason,
      this.pensionRevisionReason,
      this.otherPensionScheme,
      this.pensionerPayItem,
      this.pensionerPayItemAssignment,
      this.pensionPayrollLineItem,
      this.exitReasonDocument,
      this.memberExitDocument,
      this.pensionAgeFactor,
    ];
  }
}
